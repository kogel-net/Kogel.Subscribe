using System;
using System.Buffers;
using System.Text;
using Kogel.Slave.Mysql.Extensions;

namespace Kogel.Slave.Mysql
{
    public sealed class QueryEvent : LogEvent
    {
        public int SlaveProxyID { get; private set; }
        public DateTime ExecutionTime { get; private set; }
        public short ErrorCode { get; private set; }
        public string StatusVars { get; private set; }
        public string Schema { get; private set; }
        public String Query { get; private set; }

        public QueryEvent()
        {
            this.HasCRC = true;
        }

        protected internal override void DecodeBody(ref SequenceReader<byte> reader, object context)
        {
            reader.TryReadLittleEndian(out int slaveProxyID);
            SlaveProxyID = slaveProxyID;

            reader.TryReadLittleEndian(out int seconds);
            //ExecutionTime = LogEvent.GetTimestampFromUnixEpoch(seconds);
            //Timestamp utc time
            ExecutionTime = Timestamp.AddHours(8);

            reader.TryRead(out byte schemaLen);

            reader.TryReadLittleEndian(out short errorCode);
            ErrorCode = errorCode;

            reader.TryReadLittleEndian(out short statusVarsLen);

            StatusVars = reader.ReadString(Encoding.UTF8, statusVarsLen);

            Schema = reader.ReadString(Encoding.UTF8, schemaLen);

            reader.Advance(1); //0x00

            this.RebuildReaderAsCRC(ref reader);

            Query = reader.ReadString();
        }

        public override string ToString()
        {
            return $"{EventType}\r\nSlaveProxyID: {SlaveProxyID}\r\nExecutionTime: {ExecutionTime}\r\nErrorCode: {ErrorCode}\r\nStatusVars: {StatusVars}\r\nSchema: {Schema}\r\nQuery: {Query}";
        }
    }
}
