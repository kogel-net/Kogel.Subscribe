using Kogel.Dapper.Extension;
using Kogel.Subscribe.Mssql.Entites;
using Kogel.Subscribe.Mssql.Entites.Enum;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kogel.Subscribe.Mssql.Middleware
{
    /// <summary>
    /// Elasticsearch订阅
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ElasticsearchSubscribe<T> : ISubscribe<T>
        where T : class
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly SubscribeContext<T> _context;

        /// <summary>
        /// 
        /// </summary>
        private readonly ElasticClient _defaultClient;

        /// <summary>
        /// 拦截转换器
        /// </summary>
        private readonly Func<SubscribeMessage<T>, EsSubscribeMessage<object>> _funcWriteInterceptor;

        /// <summary>
        /// 
        /// </summary>
        private readonly List<string> _esNameList;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public ElasticsearchSubscribe(SubscribeContext<T> context)
        {
            this._context = context;
            var settings = _context.Options.ElasticsearchConfig.Settings.DefaultIndex(GetIndexName());
            this._defaultClient = new ElasticClient(settings);
            _funcWriteInterceptor = _context.Options.ElasticsearchConfig?.WriteInterceptor?.Compile();
            _esNameList = new List<string>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageList"></param>
        public void Subscribes(List<SubscribeMessage<T>> messageList)
        {
            //数据转换
            List<EsSubscribeMessage<object>> esMessageList = Interceptor(messageList, out bool isShards);
            //首次写入验证索引并创建
            if (isShards)
            {
                foreach (var message in esMessageList)
                {
                    VaildOrCreate(message.EsIndexName, message.Result.GetType());
                }
            }
            else
            {
                VaildOrCreate(GetIndexName(), typeof(T));
            }
            //消费并写入es
            foreach (var message in esMessageList)
            {
                if (message.Operation == OperationEnum.DELETE)
                {
                    //获取主键属性
                    var (idName, idProperty) = GetIdentity();
                    var id = idProperty.GetValue(message.Result);
                    if (isShards)
                        _defaultClient.Delete(DocumentPath<T>.Id(new Id(id)), i => i.Index(message.EsIndexName));
                    else
                        _defaultClient.Delete(DocumentPath<T>.Id(new Id(id)));
                }
                else
                {
                    if (isShards)
                        _defaultClient.Index(message.Result, i => i.Index(message.EsIndexName));
                    else
                        _defaultClient.IndexDocument(message.Result);
                }
            }
        }

        /// <summary>
        /// 拦截转换
        /// </summary>
        /// <param name="messageList"></param>
        /// <param name="isShards">是否分片</param>
        /// <returns></returns>
        private List<EsSubscribeMessage<object>> Interceptor(List<SubscribeMessage<T>> messageList, out bool isShards)
        {
            isShards = false;
            if (_funcWriteInterceptor != null)
            {
                isShards = true;
                return messageList.Select(message => _funcWriteInterceptor.Invoke(message)).ToList();
            }
            else
            {
                var esIndexName = GetIndexName();
                return messageList.Select(message => message.ToEsSubscribeMessage(esIndexName)).ToList();
            }
        }

        //主键名称
        private string _identityName;
        //主键反射对象
        private PropertyInfo _identityProperty;
        /// <summary>
        /// 获取主键属性
        /// </summary>
        /// <returns></returns>
        private (string, PropertyInfo) GetIdentity()
        {
            if (string.IsNullOrEmpty(_identityName))
            {
                var entity = EntityCache.QueryEntity(typeof(T));
                try
                {
                    _identityName = entity.Identitys;
                    _identityProperty = entity.EntityFieldList.FirstOrDefault(x => x.FieldName.Contains(_identityName)).PropertyInfo;
                }
                catch
                {
                    throw new Exception($"请用特性[Identity]标记表{entity.Name}的主键特性!");
                }
            }
            return (_identityName, _identityProperty);
        }

        /// <summary>
        /// es索引基本配置
        /// </summary>
        private static readonly Dictionary<string, object> _esIndexSetting = new Dictionary<string, object>
        {
             { "index.number_of_shards", "5" },
             { "index.number_of_replicas", "1" },
             { "refresh_interval", "5s" },
             { "index.max_result_window", 2000000000 },
             { "max_ngram_diff", 5 }
        };

        /// <summary>
        /// 验证创建索引
        /// </summary>
        /// <param name="esIndexName"></param>
        private void VaildOrCreate(string esIndexName, Type esType)
        {
            var objectLock = ObjectLock.CreateOrGet(esIndexName);
            lock (objectLock)
            {
                if (!_esNameList.Any(x => x == esIndexName))
                {
                    _esNameList.Add(esIndexName);
                    if (!(_defaultClient.Indices.Exists(esIndexName)).Exists)
                    {
                        var indsettings = new IndexSettings(_esIndexSetting)
                        {
                            Analysis = new Analysis
                            {
                                Analyzers = new Analyzers(),
                                Tokenizers = new Tokenizers()
                            }
                        };
                        //短内容分析设置5个字符以内
                        var shortAnalyzer = new CustomAnalyzer
                        {
                            Tokenizer = "ngram_tokenizer_short",
                            Filter = new List<string>() { "lowercase" }
                        };
                        indsettings.Analysis.Analyzers.Add("ngram_analyzer_short", shortAnalyzer);
                        indsettings.Analysis.Tokenizers.Add("ngram_tokenizer_short", new NGramTokenizer { MinGram = 1, MaxGram = 4 });
                        //长内容分析设置5个字符以上
                        var longAnalyzer = new CustomAnalyzer
                        {
                            Tokenizer = "ngram_tokenizer_long",
                            Filter = new List<string>() { "lowercase" }
                        };
                        indsettings.Analysis.Analyzers.Add("ngram_analyzer_long", longAnalyzer);
                        indsettings.Analysis.Tokenizers.Add("ngram_tokenizer_long", new NGramTokenizer { MinGram = 5, MaxGram = 5 });
                        var indexState = new IndexState { Settings = indsettings };
                        var response = _defaultClient.Indices
                            .Create(esIndexName, p => p.InitializeUsing(indexState)
                                .Map(x => x.AutoMap(esType)
                                    .Properties((propertiesSelector) =>
                                    {
                                        foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
                                        {
                                            if (!propertyInfo.CanWrite)
                                                continue;

                                            //获取字段信息
                                            var (ignore, fieldName) = GetField(propertyInfo);
                                            if (ignore)
                                                continue;

                                            string propertyTypeName = propertyInfo.PropertyType.Name;
                                            //可能是可空类型
                                            if (propertyInfo.PropertyType.FullName.Contains("System.Nullable") && propertyInfo.PropertyType.GenericTypeArguments != null && propertyInfo.PropertyType.GenericTypeArguments.Count() != 0)
                                                propertyTypeName = propertyInfo.PropertyType.GenericTypeArguments[0].Name;

                                            switch (propertyTypeName)
                                            {
                                                case nameof(Int16):
                                                case nameof(Int32):
                                                case nameof(Int64):
                                                case nameof(UInt16):
                                                case nameof(UInt32):
                                                case nameof(UInt64):
                                                case nameof(Decimal):
                                                case nameof(Single):
                                                case nameof(Double):
                                                case nameof(Byte):
                                                    propertiesSelector = propertiesSelector.Number(propertyDescriptor => propertyDescriptor.Name(fieldName));
                                                    break;

                                                case nameof(Boolean):
                                                    propertiesSelector = propertiesSelector.Boolean(propertyDescriptor => propertyDescriptor.Name(fieldName));
                                                    break;

                                                case nameof(DateTime):
                                                    propertiesSelector = propertiesSelector.Date(propertyDescriptor => propertyDescriptor.Name(fieldName));
                                                    break;

                                                case nameof(String):
                                                    propertiesSelector = propertiesSelector.Keyword(propertyDescriptor => propertyDescriptor.Name(fieldName));
                                                    break;

                                                default:
                                                    throw new Exception($"未知的数据类型{propertyTypeName}，如果不是索引内的字段请用特性[PropertyName(Ignore = true)]忽略");
                                            }
                                        }
                                        return propertiesSelector;
                                    })
                                )
                            );
                        if (!response.IsValid)
                            throw new Exception($"创建索引失败:{response.OriginalException.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 通过nest自带的特性得到索引名
        /// </summary>
        /// <returns></returns>
        private string GetIndexName()
        {
            var type = typeof(T);
            var elasticsearchType = type.GetCustomAttribute<ElasticsearchTypeAttribute>();
            if (elasticsearchType is null)
                return type.Name;
            if (!string.IsNullOrEmpty(elasticsearchType.RelationName))
                return elasticsearchType.RelationName;
#pragma warning disable CS0618 // 类型或成员已过时
            if (!string.IsNullOrEmpty(elasticsearchType.Name))
#pragma warning restore CS0618 // 类型或成员已过时
#pragma warning disable CS0618 // 类型或成员已过时
                return elasticsearchType.Name;
#pragma warning restore CS0618 // 类型或成员已过时
            return type.Name;
        }

        /// <summary>
        ///  通过nest自带的特性得到字段信息
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private (bool, string) GetField(PropertyInfo property)
        {
            bool ignore = true;
            string fieldName = property.Name;
            var propertyName = property.GetCustomAttribute<PropertyNameAttribute>();
            if (!(propertyName is null))
            {
                ignore = propertyName.Ignore;
                if (!string.IsNullOrEmpty(propertyName.Name))
                    fieldName = propertyName.Name;
            }
            return (ignore, fieldName);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
        }
    }
}
