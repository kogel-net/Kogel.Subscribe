using Kogel.Subscribe.Mssql.Entites;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Kogel.Dapper.Extension;

namespace Kogel.Subscribe.Mssql.Middleware
{
    /// <summary>
    /// rabbitmq订阅，推送到rabbitmq队列中
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RabbitMQSubscribe<T> : MiddlewareSubscribe<T>
         where T : class
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly OptionsBuilder _options;

        public RabbitMQSubscribe(OptionsBuilder options) : base(options)
        {
            this._options = options;
        }

        /// <summary>
        /// 订阅消息并推送到rabbitmq中
        /// </summary>
        /// <param name="messageList"></param>
        public override void Subscribes(List<SubscribeMessage<T>> messageList)
        {
            using (var connection = _options.RabbitMQConfig.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    //发送消息
                    string exchange = _options.TopicName ?? $"kogel_subscribe_{EntityCache.QueryEntity(typeof(T)).Name}";
                    var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageList));
                    channel.BasicPublish(exchange: exchange, mandatory: false, routingKey: "", basicProperties: null, body: body);
                }
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
