using Kogel.Subscribe.Mssql.Entites;
using System;
using System.Collections.Generic;
using Confluent.Kafka;
using Newtonsoft.Json;
using Kogel.Dapper.Extension;

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
        public KafkaSubscribe(SubscribeContext<T> context)
        {
            this._context = context;
        }

        /// <summary>
        /// 订阅消息并推送到kafka中
        /// </summary>
        /// <param name="messageList"></param>
        public void Subscribes(List<SubscribeMessage<T>> messageList)
        {
            using (var producer = new ProducerBuilder<Null, string>(_context.Options.KafkaConfig).Build())
            {
                producer.Produce(_context.Options.TopicName ?? $"kogel_subscribe_{_context.TableName}", new Message<Null, string>()
                {
                    Value = JsonConvert.SerializeObject(messageList)
                }, (result) =>
                {
                    Console.WriteLine(!result.Error.IsError ? $"推送消息到 {result.TopicPartitionOffset}" : $"推送异常: {result.Error.Reason}");
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
        }
    }
}
