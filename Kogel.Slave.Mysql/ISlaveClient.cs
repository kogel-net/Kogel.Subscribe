using System;
using System.Threading.Tasks;
using SuperSocket.Client;

namespace Kogel.Slave.Mysql
{
    public interface ISlaveClient
    {
        Task<LoginResult> ConnectAsync();
        ValueTask CloseAsync();

        event PackageHandler<LogEvent> PackageHandler;
        
        event EventHandler Closed;
    }
}
