using Confluent.Kafka;
using Kogel.Subscribe.Mssql.Entites.Enum;
using Nest;
using RabbitMQ.Client;
using System.Collections.Generic;

namespace Kogel.Subscribe.Mssql
{
    /// <summary>
    /// 
    /// </summary>
    public class OptionsBuilder
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        internal string ConnectionString { get; private set; }

        /// <summary>
        /// 扫描间隔(每次扫描变更表的间隔，单位毫秒) 默认5000毫秒/5秒
        /// </summary>
        internal int? ScanInterval { get; private set; }

        /// <summary>
        /// 变更捕捉文件在DB保存的时间（默认三天）
        /// </summary>
        internal int Retention { get; private set; } = 60 * 24 * 3;

        /// <summary>
        /// 使用哪种第三方中间件
        /// </summary>
        internal List<MiddlewareEnum> MiddlewareTypeList { get; private set; } = new List<MiddlewareEnum>();

        /// <summary>
        /// Elasticsearch配置参数
        /// </summary>
        internal ConnectionSettings ElasticsearchConfig { get; private set; }

        /// <summary>
        /// Kafka配置参数
        /// </summary>
        internal ProducerConfig KafkaConfig { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        internal ConnectionFactory RabbitMQConfig { get; private set; }

        /// <summary>
        /// 配置消息队列的topic（d当配置消息队列为rabbitmq时，此参数为配置交换机）
        /// </summary>
        internal string TopicName { get; private set; }

        /// <summary>
        /// 是否首次扫描全部
        /// </summary>
        internal bool IsFirstScanFull { get; set; } = false;

        /// <summary>
        /// 每次检索的变更量
        /// </summary>
        internal int Limit { get; set; } = 10;

        /// <summary>
        /// 是否异常中断（订阅消费过程中发生异常是否会影响后续增量捕捉）
        /// </summary>
        internal bool IsErrorInterrupt { get; set; } = true;

        /// <summary>
        /// 配置连接字符串
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public OptionsBuilder BuildConnection(string connectionString)
        {
            this.ConnectionString = connectionString;
            return this;
        }

        /// <summary>
        /// 配置扫描间隔(每次扫描变更表的间隔，单位毫秒) 默认5000毫秒/5秒
        /// </summary>
        /// <param name="scanInterval"></param>
        /// <returns></returns>
        public OptionsBuilder BuildScanInterval(int scanInterval = 5000)
        {
            this.ScanInterval = scanInterval;
            return this;
        }

        /// <summary>
        /// 变更捕捉文件在DB保存的时间（默认三天）
        /// </summary>
        /// <param name="retention">分钟</param>
        /// <returns></returns>
        public OptionsBuilder BuildRetention(int retention = 4320)
        {
            this.Retention = retention;
            return this;
        }

        /// <summary>
        /// 配置Elasticsearch推送生成数据
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public OptionsBuilder BuildElasticsearch(ConnectionSettings config)
        {
            this.MiddlewareTypeList.Add(MiddlewareEnum.Kafka);
            this.ElasticsearchConfig = config;
            return this;
        }

        /// <summary>
        /// 配置Kafka作为消息队列推送
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public OptionsBuilder BuildKafka(ProducerConfig config)
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
        public OptionsBuilder BuilderRabbitMQ(ConnectionFactory config)
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
        public OptionsBuilder BuildTopic(string topicName)
        {
            this.TopicName = topicName;
            return this;
        }

        /// <summary>
        /// 配置是否首次扫描全部（默认不扫描）
        /// </summary>
        /// <param name="isFirstScanFull"></param>
        /// <returns></returns>
        public OptionsBuilder BuildFirstScanFull(bool isFirstScanFull = false)
        {
            this.IsFirstScanFull = isFirstScanFull;
            return this;
        }

        /// <summary>
        /// 每次检索的变更最大条数（同时也是每次写到队列的数据最大条数，建议5-20条）默认10条
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public OptionsBuilder BuildLimit(int limit = 10)
        {
            this.Limit = limit;
            return this;
        }

        /// <summary>
        /// 配置是否异常中断（订阅消费过程中发生异常是否会影响后续增量捕捉）
        /// </summary>
        /// <param name="isErrorInterrupt"></param>
        /// <returns></returns>
        public OptionsBuilder BuildErrorInterrupt(bool isErrorInterrupt = true)
        {
            this.IsErrorInterrupt = isErrorInterrupt;
            return this;
        }
    }
}
