using Kogel.Slave.Mysql.Entites;
using Kogel.Slave.Mysql.Event;
using Kogel.Slave.Mysql.Socket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kogel.Slave.Mysql.Interface
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISlaveConnect
    {
        /// <summary>
        /// 连接到mysql服务器
        /// </summary>
        /// <param name="server"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="serverId"></param>
        /// <returns></returns>
        Task<LoginResult> ConnectAsync(string server, string username, string password, int serverId);

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <returns></returns>
        Task<LogEvent> ReceiveAsync();

        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <returns></returns>
        Task CloseAsync();

        /// <summary>
        /// 启动接收
        /// </summary>
        void StartReceive();

        /// <summary>
        /// 包请求事件
        /// </summary>
        event PackageHandler<LogEvent> PackageHandler;

        /// <summary>
        /// 关闭事件
        /// </summary>
        event EventHandler Closed;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TReceivePackage"></typeparam>
    /// <param name="sender"></param>
    /// <param name="package"></param>
    /// <returns></returns>
    public delegate Task PackageHandler<TReceivePackage>(SocketConnect<TReceivePackage> sender, TReceivePackage package) where TReceivePackage : class;
}
