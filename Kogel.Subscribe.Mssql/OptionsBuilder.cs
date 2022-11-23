using Confluent.Kafka;
using Kogel.Subscribe.Mssql.Entites;
using Kogel.Subscribe.Mssql.Entites.Enum;
using Nest;
using RabbitMQ.Client;
using System.Collections.Generic;

namespace Kogel.Subscribe.Mssql
{
    /// <summary>
    /// 
    /// </summary>
    public class OptionsBuilder<T>
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        internal string ConnectionString { get; private set; }

        /// <summary>
        /// Cdc监听设置
        /// </summary>
        internal CdcConfig CdcConfig { get; private set; } = new CdcConfig();

        /// <summary>
        /// 表分片
        /// </summary>
        internal List<string> Shards { get; set; }

        /// <summary>
        /// 使用哪种第三方中间件
        /// </summary>
        internal List<MiddlewareEnum> MiddlewareTypeList { get; private set; } = new List<MiddlewareEnum>();

        /// <summary>
        /// Elasticsearch配置参数
        /// </summary>
        internal ElasticsearchConfig<T> ElasticsearchConfig { get; private set; }

        /// <summary>
        /// Kafka配置参数
        /// </summary>
        internal ProducerConfig KafkaConfig { get; private set; }

        /// <summary>
        /// RabbitMQ配置参数
        /// </summary>
        internal ConnectionFactory RabbitMQConfig { get; private set; }

        /// <summary>
        /// 配置消息队列的topic（d当配置消息队列为rabbitmq时，此参数为配置交换机）
        /// </summary>
        internal string TopicName { get; private set; }

        /// <summary>
        /// 配置连接字符串
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public OptionsBuilder<T> BuildConnection(string connectionString)
        {
            this.ConnectionString = connectionString;
            return this;
        }

        /// <summary>
        /// 配置Cdc监听设置
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public OptionsBuilder<T> BuildCdcConfig(CdcConfig config)
        {
            this.CdcConfig = config;
            return this;
        }

        /// <summary>
        /// 配置所有表分片
        /// </summary>
        /// <param name="shards">所有分表表名</param>
        /// <returns></returns>
        public OptionsBuilder<T> BuildShards(List<string> shards)
        {
            this.Shards = shards;
            return this;
        }

        /// <summary>
        /// 配置Elasticsearch推送生成数据
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public OptionsBuilder<T> BuildElasticsearch(ElasticsearchConfig<T> config)
        {
            this.MiddlewareTypeList.Add(MiddlewareEnum.Elasticsearch);
            this.ElasticsearchConfig = config;
            return this;
        }

        /// <summary>
        /// 配置Kafka作为消息队列推送
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public OptionsBuilder<T> BuildKafka(ProducerConfig config)
        {
            this.MiddlewareTypeList.Add(MiddlewareEnum.Kafka);
            this.KafkaConfig = config;
            return this;
        }

        /// <summary>
        /// 配置Rabbitmq作为消息队列推送
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public OptionsBuilder<T> BuilderRabbitMQ(ConnectionFactory config)
        {
            this.MiddlewareTypeList.Add(MiddlewareEnum.RabbitMQ);
            this.RabbitMQConfig = config;
            return this;
        }

        /// <summary>
        /// 配置消息队列的topic（d当配置消息队列为rabbitmq时，此参数为配置交换机）
        /// </summary>
        /// <param name="topicName"></param>
        /// <returns></returns>
        public OptionsBuilder<T> BuildTopic(string topicName)
        {
            this.TopicName = topicName;
            return this;
        }
    }
}
