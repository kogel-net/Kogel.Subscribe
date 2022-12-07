using Nest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;

namespace Kogel.Subscribe.Mssql.Entites
{
    /// <summary>
    /// Elasticsearch配置参数
    /// </summary>
    public class ElasticsearchConfig<T>
    {
        /// <summary>
        /// Nest配置
        /// </summary>
        public ConnectionSettings Settings { get; set; }

        /// <summary>
        /// Es写入拦截
        /// </summary>
        public Expression<Func<SubscribeMessage<T>, EsSubscribeMessage<object>>> WriteInterceptor { get; set; }
    }
}
