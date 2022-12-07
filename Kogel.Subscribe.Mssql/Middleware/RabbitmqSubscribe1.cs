using Kogel.Subscribe.Mssql.Entites;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Kogel.Dapper.Extension;
using RabbitMQ.Client;
using System.Threading;

namespace Kogel.Subscribe.Mssql.Middleware
{
    /// <summary>
    /// rabbitmq订阅，推送到rabbitmq队列中
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RabbitmqSubscribe1<T> : ISubscribe<T>
         where T : class
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly SubscribeContext<T> _context;

        /// <summary>
        /// 
        /// </summary>
        private readonly ThreadLocal<IConnection> _connectionLocal;
        public RabbitmqSubscribe1(SubscribeContext<T> context)
        {
            this._context = context;
            this._connectionLocal = new ThreadLocal<IConnection>(true);
        }

        /// <summary>
        /// 订阅消息并推送到rabbitmq中
        /// </summary>
        /// <param name="messageList"></param>
        public void Subscribes(List<SubscribeMessage<T>> messageList)
        {
            if (_connectionLocal.Value is null)
                _connectionLocal.Value = _context.Options.RabbitMQConfig.CreateConnection();
            using (var channel = _connectionLocal.Value.CreateModel())
            {
                string exchange = _context.Options.TopicName ?? $"kogel_subscribe_exchange_{_context.TableName}";
                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageList));
                channel.BasicPublish(exchange: exchange, mandatory: false, routingKey: "", basicProperties: null, body: body);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (!(_connectionLocal.Values is null))
                foreach (var connection in _connectionLocal.Values)
                    connection.Dispose();
        }
    }
}
