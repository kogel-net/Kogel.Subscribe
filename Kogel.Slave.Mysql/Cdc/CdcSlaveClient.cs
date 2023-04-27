using Microsoft.Extensions.Options;
using SuperSocket.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kogel.Slave.Mysql.Cdc
{
    public sealed class CdcSlaveClient : IAsyncDisposable
    {
        private readonly SlaveClient _slaveClient;

        private readonly CdcClientOptions _options;

        public CdcSlaveClient(CdcClientOptions options)
        {
            _slaveClient = new SlaveClient();
            _options = options;
        }

        public async Task ConnectAsync()
        {
            _slaveClient.PackageHandler += CdcPackageHandler;
            await _slaveClient.ConnectAsync(_options);
        }

        public async ValueTask DisposeAsync()
        {
            await _slaveClient.CloseAsync();
        }

        private ValueTask CdcPackageHandler(EasyClient<LogEvent> sender, LogEvent package)
        {
            return default;
        }
    }
}
