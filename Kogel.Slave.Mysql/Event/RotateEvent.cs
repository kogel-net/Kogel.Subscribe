using Kogel.Slave.Mysql.Extension;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kogel.Slave.Mysql.Event
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class RotateEvent : LogEvent
    {
        public long RotatePosition { get; set; }

        public string NextBinlogFileName { get; set; }

        protected internal override void DecodeBody(ref SequenceReader<byte> reader, object context)
        {
            reader.TryReadLittleEndian(out short position);
            RotatePosition = position;
            var binglogFileNameSize = reader.Remaining - (int)BinLogCheckSum;
            NextBinlogFileName = reader.Sequence.Slice(reader.Consumed, binglogFileNameSize).GetString(Encoding.UTF8);
            reader.Advance(binglogFileNameSize);
        }

        public override string ToString()
        {
            return $"{LogEventType}\r\nRotatePosition: {RotatePosition}\r\nNextBinlogFileName: {NextBinlogFileName}";
        }
    }
}
