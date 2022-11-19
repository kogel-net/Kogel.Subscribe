using Kogel.Dapper.Extension;
using Kogel.Subscribe.Mssql.Entites;
using Kogel.Subscribe.Mssql.Entites.Enum;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Kogel.Subscribe.Mssql.Middleware
{
    /// <summary>
    /// Elasticsearch订阅
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ElasticsearchSubscribe<T> : MiddlewareSubscribe<T>
        where T : class
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly OptionsBuilder _options;

        /// <summary>
        /// 
        /// </summary>
        private readonly ElasticClient _client;

        public ElasticsearchSubscribe(OptionsBuilder options) : base(options)
        {
            this._options = options;
            var settings = _options.ElasticsearchConfig.DefaultIndex(GetIndexName());
            this._client = new ElasticClient(settings);
        }

        private bool _isFirstSubscribe = true;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageList"></param>
        public override void Subscribes(List<SubscribeMessage<T>> messageList)
        {
            if (_isFirstSubscribe)
            {
                _isFirstSubscribe = false;
                //验证索引是否存在并创建索引
                CreateIndex(_client);
            }
            foreach (var message in messageList)
            {
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
        /// 创建索引
        /// </summary>
        /// <param name="elasticClient"></param>
        /// <param name="indexName"></param>
        private void CreateIndex(ElasticClient elasticClient)
        {
            string indexName = GetIndexName();
            if (!(elasticClient.Indices.Exists(indexName)).Exists)
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
                var response = elasticClient.Indices
                    .Create(indexName, p => p.InitializeUsing(indexState)
                        .Map<T>(x => x.AutoMap<T>()
                            .Properties<T>((propertiesSelector) =>
                            {
                                foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
                                {
                                    if (!propertyInfo.CanWrite)
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
                                            propertiesSelector = propertiesSelector.Number(propertyDescriptor => propertyDescriptor.Name(propertyInfo.Name));
                                            break;

                                        case nameof(Boolean):
                                            propertiesSelector = propertiesSelector.Boolean(propertyDescriptor => propertyDescriptor.Name(propertyInfo.Name));
                                            break;

                                        case nameof(DateTime):
                                            propertiesSelector = propertiesSelector.Date(propertyDescriptor => propertyDescriptor.Name(propertyInfo.Name));
                                            break;

                                        case nameof(String):
                                            propertiesSelector = propertiesSelector.Keyword(propertyDescriptor => propertyDescriptor.Name(propertyInfo.Name));
                                            break;

                                        default:
                                            break;
                                    }
                                }
                                return propertiesSelector;
                            })
                        )
                    );
                if (!response.IsValid)
                {
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
        /// 
        /// </summary>
        public override void Dispose()
        {
        }
    }
}
