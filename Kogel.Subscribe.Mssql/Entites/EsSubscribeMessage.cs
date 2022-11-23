using System;
using System.Collections.Generic;
using System.Text;

namespace Kogel.Subscribe.Mssql.Entites
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EsSubscribeMessage<T> : SubscribeMessage<T>
    {
        /// <summary>
        /// 索引名称
        /// </summary>
        public string EsIndexName { get; set; }
    }
}
