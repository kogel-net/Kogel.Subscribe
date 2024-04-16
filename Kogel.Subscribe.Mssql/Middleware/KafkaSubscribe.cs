using Kogel.Subscribe.Mssql.Entites;

namespace Kogel.Subscribe.Mssql.Middleware
{
    /// <summary>
    /// kafka订阅，推送到kafka队列中
    /// </summary>
    public class KafkaSubscribe<T> : ISubscribe<T>
        where T : class
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly SubscribeContext<T> _context;

        /// <summary>
        /// 
        /// </summary>
        private readonly ThreadLocal<IProducer<Null, string>> _producerLocal;
        public KafkaSubscribe(SubscribeContext<T> context)
        {
            this._context = context;
            this._producerLocal = new ThreadLocal<IProducer<Null, string>>(true);
        }

        /// <summary>
        /// 订阅消息并推送到kafka中
        /// </summary>
        /// <param name="messageList"></param>
        public void Subscribes(List<SubscribeMessage<T>> messageList)
        {
            if (_producerLocal.Value is null)
                _producerLocal.Value = new ProducerBuilder<Null, string>(_context.Options.KafkaConfig).Build();
            string topic = _context.Options.TopicName ?? $"kogel_subscribe_topic_{_context.TableName}";
            _producerLocal.Value.Produce(topic, new Message<Null, string>()
            {
                Value = JsonConvert.SerializeObject(messageList)
            }, (result) =>
            {
                Console.WriteLine(!result.Error.IsError ? $"推送消息到 {result.TopicPartitionOffset}" : $"推送异常: {result.Error.Reason}");
            });

        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (!(_producerLocal.Values is null))
                foreach (var producer in _producerLocal.Values)
                    producer.Dispose();
        }
    }
}
