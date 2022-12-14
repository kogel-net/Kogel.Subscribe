using System;
using System.Buffers;
using System.Text;
using SuperSocket.ProtoBase;

namespace Kogel.Slave.Mysql
{
    public sealed class ErrorEvent : LogEvent
    {
        public short ErrorCode { get; private set; }
        public string SqlState { get; private set; }
        public String ErrorMessage { get; private set; }
        protected internal override void DecodeBody(ref SequenceReader<byte> reader, object context)
        {
            reader.TryReadLittleEndian(out short errorCode);

            ErrorCode = errorCode;

            reader.TryPeek(out byte checkValue);

            if (checkValue == '#')
            {
                reader.Advance(1);
                SqlState = reader.Sequence.Slice(reader.Consumed, 5).GetString(Encoding.UTF8);
                reader.Advance(5);
            }

            ErrorMessage = reader.Sequence.Slice(reader.Consumed).GetString(Encoding.UTF8);
        }

        public override string ToString()
        {
            return $"{EventType.ToString()}\r\nSqlState: {SqlState}\r\nErrorMessage: {ErrorMessage}";
        }
    }
}
