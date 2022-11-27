using Kogel.Dapper.Extension;
using Kogel.Subscribe.Mssql.Entites;
using Kogel.Subscribe.Mssql.Entites.Enum;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

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
        private ElasticClient _client;

        /// <summary>
        /// 拦截转换器
        /// </summary>
        private readonly Func<SubscribeMessage<T>, EsSubscribeMessage<T>> _funcWriteInterceptor;

        public ElasticsearchSubscribe(SubscribeContext<T> context)
        {
            this._context = context;
            this._client = GetClient();
            _funcWriteInterceptor = _context._options.ElasticsearchConfig?.WriteInterceptor?.Compile();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="esIndexName"></param>
        /// <returns></returns>
        private ElasticClient GetClient(string esIndexName = null)
        {
            var settings = _context._options.ElasticsearchConfig.Settings.DefaultIndex(esIndexName ?? GetIndexName());
            return new ElasticClient(settings);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageList"></param>
        public void Subscribes(List<SubscribeMessage<T>> messageList)
        {
            //数据转换
            List<EsSubscribeMessage<T>> esMessageList = Interceptor(messageList, out bool isShards);
            //首次写入验证索引并创建
            var esIndexNames = isShards ? esMessageList.Select(x => x.EsIndexName).Distinct().ToList() : null;
            CheckFirst(esIndexNames);
            //消费并写入es
            foreach (var message in esMessageList)
            {
                if (isShards)
                    _client = GetClient(message.EsIndexName);
                if (message.Operation == OperationEnum.DELETE)
                {
                    //获取主键属性
                    var (idName, idProperty) = GetIdentity();
                    var id = idProperty.GetValue(message.Result);
                    _client.Delete(DocumentPath<T>.Id(id.ToString()));
                }
                else
                {
                    _client.IndexDocument(message.Result);
                }
            }
        }

        /// <summary>
        /// 拦截转换
        /// </summary>
        /// <param name="messageList"></param>
        /// <param name="isShards">是否分片</param>
        /// <returns></returns>
        private List<EsSubscribeMessage<T>> Interceptor(List<SubscribeMessage<T>> messageList, out bool isShards)
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

        private bool _isFirstSubscribe = true;
        /// <summary>
        /// 
        /// </summary>
        private void CheckFirst(List<string> shardIndexNames = null)
        {
            if (_isFirstSubscribe)
            {
                _isFirstSubscribe = false;
                //验证索引是否存在并创建索引
                if (shardIndexNames == null)
                    VaildOrCreate();
                else
                    shardIndexNames.ForEach(x => VaildOrCreate(x));
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

        private static readonly object _createIndexLock = new object();
        /// <summary>
        /// 验证创建索引
        /// </summary>
        /// <param name="esIndexName"></param>
        private void VaildOrCreate(string esIndexName = null)
        {
            lock (_createIndexLock)
            {
                string indexName = esIndexName ?? GetIndexName();
                if (!(_client.Indices.Exists(indexName)).Exists)
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
                    var response = _client.Indices
                        .Create(indexName, p => p.InitializeUsing(indexState)
                            .Map<T>(x => x.AutoMap<T>()
                                .Properties<T>((propertiesSelector) =>
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
            if (!string.IsNullOrEmpty(elasticsearchType.Name))
                return elasticsearchType.Name;
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
