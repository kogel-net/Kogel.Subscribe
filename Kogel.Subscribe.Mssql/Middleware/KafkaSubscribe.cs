﻿using Kogel.Subscribe.Mssql.Entites;
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
    public class KafkaSubscribe<T> : MiddlewareSubscribe<T>
        where T : class
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly OptionsBuilder _options;

        public KafkaSubscribe(OptionsBuilder options) : base(options)
        {
            this._options = options;
        }

        /// <summary>
        /// 订阅消息并推送到kafka中
        /// </summary>
        /// <param name="messageList"></param>
        public override void Subscribes(List<SubscribeMessage<T>> messageList)
        {
            using (var producer = new ProducerBuilder<Null, string>(_options.KafkaConfig).Build())
            {
                producer.Produce(_options.TopicName ?? $"kogel_subscribe_{EntityCache.QueryEntity(typeof(T)).Name}", new Message<Null, string>()
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
        public override void Dispose()
        {
        }
    }
}