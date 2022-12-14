using System;
using System.Buffers;
using System.Text;
using SuperSocket.ProtoBase;

namespace Kogel.Slave.Mysql
{
    public sealed class XIDEvent : LogEvent
    {
        public long TransactionID { get; set; }
        
        protected internal override void DecodeBody(ref SequenceReader<byte> reader, object context)
        {
            reader.TryReadLittleEndian(out short tarnsID);
            TransactionID = tarnsID;
        }
    }
}
