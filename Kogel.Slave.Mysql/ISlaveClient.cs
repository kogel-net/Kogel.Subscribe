using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using SuperSocket.Client;

namespace Kogel.Slave.Mysql
{
    public interface ISlaveClient
    {
        Task<LoginResult> ConnectAsync(string server, string username, string password, int serverId);
        ValueTask CloseAsync();

        event PackageHandler<LogEvent> PackageHandler;
        
        event EventHandler Closed;
    }
}
