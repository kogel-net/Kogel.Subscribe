using Kogel.Subscribe.Mssql.Entites;
using System;
using System.Collections.Generic;

namespace Kogel.Subscribe.Mssql
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISubscribe<T> : IDisposable
        where T : class
    {
        /// <summary>
        /// 订阅
        /// </summary>
        /// <param name="messageList">变更的数据</param>
        void Subscribes(List<SubscribeMessage<T>> messageList);
    }
}
