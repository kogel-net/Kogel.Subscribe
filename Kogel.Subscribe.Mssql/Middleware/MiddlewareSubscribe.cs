using Kogel.Subscribe.Mssql.Entites;
using Kogel.Subscribe.Mssql.Entites.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kogel.Subscribe.Mssql.Middleware
{
    /// <summary>
    /// 
    /// </summary>
    public class MiddlewareSubscribe<T> : ISubscribe<T>
        where T : class
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly OptionsBuilder _options;

        /// <summary>
        /// 
        /// </summary>
        private List<ISubscribe<T>> _queueSubscribeList;
        public MiddlewareSubscribe(OptionsBuilder options)
        {
            this._options = options;
        }

        /// <summary>
        /// 中间件注册
        /// </summary>
        /// <returns></returns>
        public MiddlewareSubscribe<T> Register()
        {
            this._queueSubscribeList = new List<ISubscribe<T>>();
            if (_options.MiddlewareTypeList.Any())
            {
                foreach (var middlewareType in _options.MiddlewareTypeList)
                {
                    ISubscribe<T> queueSubscribe;
                    switch (middlewareType)
                    {
                        case MiddlewareEnum.Elasticsearch:
                            queueSubscribe = new ElasticsearchSubscribe<T>(_options);
                            break;
                        case MiddlewareEnum.Kafka:
                            queueSubscribe = new KafkaSubscribe<T>(_options);
                            break;
                        case MiddlewareEnum.RabbitMQ:
                            queueSubscribe = new RabbitMQSubscribe<T>(_options);
                            break;
                        default:
                            throw new Exception($"未实现的中间件订阅【{middlewareType}】");
                    }
                    _queueSubscribeList.Add(queueSubscribe);
                }
            }
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageList"></param>
        public virtual void Subscribes(List<SubscribeMessage<T>> messageList)
        {
            this._queueSubscribeList?.ForEach(x => x?.Subscribes(messageList));
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Dispose()
        {
            this._queueSubscribeList?.ForEach(x => x?.Dispose());
        }
    }
}
