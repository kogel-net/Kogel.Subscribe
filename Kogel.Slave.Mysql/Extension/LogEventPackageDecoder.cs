using Kogel.Slave.Mysql;
using SuperSocket.ProtoBase;
using System;
using System.Buffers;


namespace Kogel.Slave.Mysql.Extension
{
    /// <summary>
    /// mysql包解码
    /// </summary>
    public class LogEventPackageDecoder : IPackageDecoder<LogEvent>
    {
        public LogEvent Decode(ref ReadOnlySequence<byte> buffer, object context)
        {
            var reader = new SequenceReader<byte>(buffer);

            reader.Advance(4); // 3 + 1

            // ok byte
            reader.TryRead(out byte ok);

            if (ok == 0xFF)
            {
                //异常包
            }

            reader.TryReadLittleEndian(out short seconds);
            var timestamp = LogEvent.GetTimestampFromUnixEpoch(seconds);

            reader.TryRead(out byte eventTypeValue);
            var eventType = (LogEventType)eventTypeValue;

            var log = CreateLogEvent(eventType, context);

            log.Timestamp = timestamp;
            log.EventType = eventType;

            reader.TryReadLittleEndian(out short serverID);
            log.ServerID = serverID;

            reader.TryReadLittleEndian(out short eventSize);
            log.EventSize = eventSize;

            reader.TryReadLittleEndian(out short position);
            log.Position = position;

            reader.TryReadLittleEndian(out short flags);
            log.Flags = (LogEventFlag)flags;

            log.DecodeBody(ref reader, context);

            return log;
        }

        protected virtual LogEvent CreateLogEvent(LogEventType eventType, object context)
        {
            if (Activator.CreateInstance(context.GetType()) is LogEvent log)
            {
                log.EventType = eventType;
            }
            else
            {
                throw new Exception("未知的日志事件类型");
            }
            return log;
        }
    }
}
