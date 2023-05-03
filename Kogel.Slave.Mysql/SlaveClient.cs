using System;
using System.IO;
using System.Threading.Tasks;
using System.Reflection;
using System.Buffers.Binary;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using SuperSocket.Channel;
using SuperSocket.Client;
using Kogel.Dapper.Extension;
using System.Linq;
using System.Data;
using Kogel.Slave.Mysql.Extensions;

namespace Kogel.Slave.Mysql
{
    public class SlaveClient : EasyClient<LogEvent>, ISlaveClient
    {
        private const byte CMD_DUMP_BINLOG = 0x12;

        private MySqlConnection _connection;

        private Stream _stream;

        public new ILogger Logger
        {
            get { return base.Logger; }
            set { base.Logger = value; }
        }

        static SlaveClient()
        {
            LogEventPackageDecoder.RegisterEmptyPayloadEventTypes(
                    LogEventType.STOP_EVENT,
                    LogEventType.INTVAR_EVENT,
                    LogEventType.SLAVE_EVENT,
                    LogEventType.RAND_EVENT,
                    LogEventType.USER_VAR_EVENT,
                    LogEventType.DELETE_ROWS_EVENT_V0,
                    LogEventType.UPDATE_ROWS_EVENT_V0,
                    LogEventType.WRITE_ROWS_EVENT_V0,
                    LogEventType.HEARTBEAT_LOG_EVENT,
                    LogEventType.ANONYMOUS_GTID_LOG_EVENT);

            LogEventPackageDecoder.RegisterLogEventType<RotateEvent>(LogEventType.ROTATE_EVENT);
            LogEventPackageDecoder.RegisterLogEventType<FormatDescriptionEvent>(LogEventType.FORMAT_DESCRIPTION_EVENT);
            LogEventPackageDecoder.RegisterLogEventType<TableMapEvent>(LogEventType.TABLE_MAP_EVENT);
            LogEventPackageDecoder.RegisterLogEventType<QueryEvent>(LogEventType.QUERY_EVENT);
            LogEventPackageDecoder.RegisterLogEventType<WriteRowsEvent>(LogEventType.WRITE_ROWS_EVENT);
            LogEventPackageDecoder.RegisterLogEventType<DeleteRowsEvent>(LogEventType.DELETE_ROWS_EVENT);
            LogEventPackageDecoder.RegisterLogEventType<UpdateRowsEvent>(LogEventType.UPDATE_ROWS_EVENT);
            LogEventPackageDecoder.RegisterLogEventType<XIDEvent>(LogEventType.XID_EVENT);
        }

        public SlaveClient()
            : base(new LogEventPipelineFilter())
        {

        }

        private Stream GetStreamFromMySQLConnection()
        {
            var driverField = _connection.GetType().GetField("driver", BindingFlags.Instance | BindingFlags.NonPublic);
            var driver = driverField.GetValue(_connection);
            var handlerField = driver.GetType().GetField("handler", BindingFlags.Instance | BindingFlags.NonPublic);
            var handler = handlerField.GetValue(driver);
            var baseStreamField = handler.GetType().GetField("baseStream", BindingFlags.Instance | BindingFlags.NonPublic);
            return baseStreamField.GetValue(handler) as Stream;
        }

        public async Task<LoginResult> ConnectAsync(ClientOptions options)
        {
            var connString = $"Server={options.Server};PORT={options.Port}; UID={options.UserName}; Password={options.Password}";
            _connection = new MySqlConnection(connString);
            await _connection.OpenAsync();
            try
            {
                //检查mysql版本
                await CheckVersion(options);

                //检查binlog配置
                await CheckBinLog();

                //获取binlog位置
                var binlogInfo = await GetBinlogFileNameAndPosition();
                //检查binlog
                var binlogChecksum = await GetBinlogChecksum();
                await ConfirmChecksum();

                LogEvent.ChecksumType = binlogChecksum;
                _stream = GetStreamFromMySQLConnection();

                //检查ServerId
                await CheckServerId(options);

                //发送dump到master
                await StartDumpBinlog(_stream, options.ServerId.Value, binlogInfo.Item1, binlogInfo.Item2);

                //设置频道
                SetupChannel(new StreamPipeChannel<LogEvent>(_stream, null,
                    new LogEventPipelineFilter
                    {
                        Context = new SlaveState()
                    },
                    new ChannelOptions
                    {
                        Logger = Logger
                    }));

                //开始接收消息
                base.StartReceive();
                return new LoginResult { Result = true };
            }
            catch (Exception e)
            {
                await _connection.CloseAsync();

                return new LoginResult
                {
                    Result = false,
                    Message = e.Message
                };
            }
        }

        private async Task CheckVersion(ClientOptions options)
        {
            if (options.Version == null)
            {
                string version = await _connection.ExecuteScalarAsync<string>("SELECT VERSION()");
                Version versionType = version.StartsWith("8") ? Version.EightPlus : Version.FivePlus;
                versionType.SetVersionEnvironmentVariable();
            }
            else
            {
                options.Version.Value.SetVersionEnvironmentVariable();
            }
        }

        private async Task CheckBinLog()
        {
            //check binlog open
            var logBinVars = await _connection.QueryFirstOrDefaultAsync<Variables>("SHOW GLOBAL VARIABLES LIKE 'log_bin'");
            if (logBinVars.Value != "ON")
            {
                throw new Exception("Confirm whether binlog is open!");
            }
            var binLogVars = await _connection.QueryAsync<Variables>("SHOW GLOBAL VARIABLES like 'binlog%'");
            if (binLogVars.Any())
            {
                foreach (var binLogVarsItem in binLogVars)
                {
                    await CheckBinLogConfig(binLogVarsItem);
                }
            }
        }

        private async Task CheckBinLogConfig(Variables variables)
        {
            switch (variables.Variable_name)
            {
                case "binlog_format":
                    await CheckBinlogFormat(variables);
                    break;

                case "binlog_row_metadata":
                    await CheckBinlogRowMetaData(variables);
                    break;

                default:
                    break;
            }
        }

        /*mysql binlog 分为三种模式（STATEMENT，ROW，MIXED），最少需要使用到 ROW*/
        private async Task CheckBinlogFormat(Variables variables)
        {
            if (variables.Value != "ROW" && variables.Value != "MIXED")
            {
                await _connection.ExecuteAsync("SET GLOBAL binlog_format = 'ROW'");
            }
        }

        private async Task CheckBinlogRowMetaData(Variables variables)
        {
            if (variables.Value != "FULL")
            {
                await _connection.ExecuteAsync("SET GLOBAL binlog_row_metadata = 'FULL'");
            }
        }

        private async Task CheckServerId(ClientOptions options)
        {
            if (!options.ServerId.HasValue)
            {
                var variables = await _connection.QueryAsync<Variables>("SHOW VARIABLES LIKE 'server_id'");
                options.ServerId = variables.Any() ? variables.Max(x => Convert.ToInt32(x.Value)) + 1 : 2;
            }
        }

        //https://dev.mysql.com/doc/refman/5.6/en/replication-howto-masterstatus.html
        private async Task<Tuple<string, int>> GetBinlogFileNameAndPosition()
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SHOW MASTER STATUS;";

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (!await reader.ReadAsync())
                    throw new Exception("No binlog information has been returned.");

                var fileName = reader.GetString(0);
                var position = reader.GetInt32(1);

                await reader.CloseAsync();

                return new Tuple<string, int>(fileName, position);
            }
        }

        private async Task<ChecksumType> GetBinlogChecksum()
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "show global variables like 'binlog_checksum';";

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (!await reader.ReadAsync())
                    return ChecksumType.NONE;

                var checksumTypeName = reader.GetString(1).ToUpper();
                await reader.CloseAsync();

                return (ChecksumType)Enum.Parse(typeof(ChecksumType), checksumTypeName);
            }
        }

        private async ValueTask ConfirmChecksum()
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "set @`master_binlog_checksum` = @@binlog_checksum;";
            await cmd.ExecuteNonQueryAsync();
        }

        /*
        https://dev.mysql.com/doc/internals/en/com-binlog-dump.html
        */
        private Memory<byte> GetDumpBinlogCommand(int serverId, string fileName, int position)
        {
            var fixPartSize = 15;
            var encoding = System.Text.Encoding.ASCII;
            var buffer = new byte[fixPartSize + encoding.GetByteCount(fileName) + 1];

            Span<byte> span = buffer;

            buffer[4] = CMD_DUMP_BINLOG;

            var n = span.Slice(5);
            BinaryPrimitives.WriteInt32LittleEndian(n, position);

            var flags = (short)0;
            n = n.Slice(4);
            BinaryPrimitives.WriteInt16LittleEndian(n, flags);

            n = n.Slice(2);
            BinaryPrimitives.WriteInt32LittleEndian(n, serverId);

            var nameSpan = n.Slice(4);

            var len = encoding.GetBytes(fileName, nameSpan);

            len += fixPartSize;

            // end of the file name
            buffer[len++] = 0x00;

            var contentLen = len - 4;

            buffer[0] = (byte)(contentLen & 0xff);
            buffer[1] = (byte)(contentLen >> 8);
            buffer[2] = (byte)(contentLen >> 16);

            return new Memory<byte>(buffer, 0, len);
        }

        private async ValueTask StartDumpBinlog(Stream stream, int serverId, string fileName, int position)
        {
            var data = GetDumpBinlogCommand(serverId, fileName, position);
            await stream.WriteAsync(data);
            await stream.FlushAsync();
        }

        public override async ValueTask CloseAsync()
        {
            var connection = _connection;

            if (connection != null)
            {
                _connection = null;
                await connection.CloseAsync();
            }

            await base.CloseAsync();
        }
    }
}