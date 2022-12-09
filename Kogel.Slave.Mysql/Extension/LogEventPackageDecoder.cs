using Kogel.Slave.Mysql.Entites.Enum;
using Kogel.Slave.Mysql.Event;
using Kogel.Slave.Mysql.Interface;
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
            var eventType = (LogEventTypeEnum)eventTypeValue;

            var log = CreateLogEvent(eventType, context);

            log.Timestamp = timestamp;
            log.LogEventType = eventType;

            reader.TryReadLittleEndian(out short serverID);
            log.ServerId = serverID;

            reader.TryReadLittleEndian(out short eventSize);
            log.LogEventSize = eventSize;

            reader.TryReadLittleEndian(out short position);
            log.LogPosition = position;

            reader.TryReadLittleEndian(out short flags);
            log.LogEventFlag = (LogEventFlagEnum)flags;

            log.DecodeBody(ref reader, context);

            return log;
        }

        protected virtual LogEvent CreateLogEvent(LogEventTypeEnum eventType, object context)
        {
            if (Activator.CreateInstance(context.GetType()) is LogEvent log)
            {
                log.LogEventType = eventType;
            }
            else
            {
                throw new Exception("未知的日志事件类型");
            }
            return log;
        }
    }
}
