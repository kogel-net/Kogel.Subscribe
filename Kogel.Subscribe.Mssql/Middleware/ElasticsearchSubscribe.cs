using Kogel.Subscribe.Mssql.Entites;
using System;
using System.Collections.Generic;
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

        public ElasticsearchSubscribe(OptionsBuilder options) : base(options)
        {
            this._options = options;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageList"></param>
        public override void Subscribes(List<SubscribeMessage<T>> messageList)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
        }
    }
}
