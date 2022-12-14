using System;
using System.Buffers;

namespace Kogel.Slave.Mysql
{
    public sealed class EmptyPayloadEvent : LogEvent
    {
        protected internal override void DecodeBody(ref SequenceReader<byte> reader, object context)
        {
            
        }
    }
}
