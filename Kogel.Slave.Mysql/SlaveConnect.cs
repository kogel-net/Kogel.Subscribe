using Kogel.Slave.Mysql.Entites;
using Kogel.Slave.Mysql.Event;
using Kogel.Slave.Mysql.Interface;
using Kogel.Slave.Mysql.Socket;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Kogel.Slave.Mysql
{
    /// <summary>
    /// 从节点连接
    /// </summary>
    public class SlaveConnect : SocketConnect<LogEvent>, ISlaveConnect
    {
        /// <summary>
        /// 
        /// </summary>
        private const byte _dump_binlog = 0x12;

        /// <summary>
        /// 
        /// </summary>
        private MySqlConnection _connection;

        /// <summary>
        /// 
        /// </summary>
        private Stream _stream;

        public event PackageHandler<LogEvent> PackageHandler;
        public event EventHandler Closed;

        public Task CloseAsync()
        {
            throw new NotImplementedException();
        }

        public Task<LoginResult> ConnectAsync(string server, string username, string password, int serverId)
        {
            throw new NotImplementedException();
        }

        public Task<LogEvent> ReceiveAsync()
        {
            throw new NotImplementedException();
        }

        public void StartReceive()
        {
            throw new NotImplementedException();
        }
    }
}
