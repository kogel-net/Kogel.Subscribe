using Kogel.Subscribe.Mssql.Entites;
using Kogel.Subscribe.Mssql.Entites.Enum;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kogel.Subscribe.Mssql.Middleware
{
    /// <summary>
    /// 中间件订阅中心
    /// </summary>
    public sealed class MiddlewareSubscribe<T> : ISubscribe<T>
        where T : class
    {

        /// <summary>
        /// 
        /// </summary>
        private List<ISubscribe<T>> _middlewareSubscribeList;

        /// <summary>
        /// 
        /// </summary>
        private readonly SubscribeContext<T> _context;

        public MiddlewareSubscribe(SubscribeContext<T> context)
        {
            this._context = context;
        }

        /// <summary>
        /// 中间件注册
        /// </summary>
        /// <returns></returns>
        public MiddlewareSubscribe<T> Register()
        {
            this._middlewareSubscribeList = new List<ISubscribe<T>> 
            {
                new VolumeSubscribe<T>(_context)
            };
            //注册选择的中间件
            if (_context.Options.MiddlewareTypeList.Any())
            {
                foreach (var middlewareType in _context.Options.MiddlewareTypeList.Distinct())
                {
                    ISubscribe<T> queueSubscribe;
                    switch (middlewareType)
                    {
                        case MiddlewareEnum.Elasticsearch:
                            queueSubscribe = new ElasticsearchSubscribe<T>(_context);
                            break;
                        case MiddlewareEnum.Kafka:
                            queueSubscribe = new KafkaSubscribe<T>(_context);
                            break;
                        case MiddlewareEnum.RabbitMQ:
                            queueSubscribe = new RabbitMQSubscribe<T>(_context);
                            break;
                        default:
                            throw new Exception($"未实现的中间件订阅【{middlewareType}】");
                    }
                    _middlewareSubscribeList.Add(queueSubscribe);
                }
            }
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageList"></param>
        public void Subscribes(List<SubscribeMessage<T>> messageList)
        {
            this._middlewareSubscribeList?.ForEach(x => x?.Subscribes(messageList));
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            this._middlewareSubscribeList?.ForEach(x => x?.Dispose());
        }
    }
}
