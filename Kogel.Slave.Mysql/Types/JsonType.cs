using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Buffers;

namespace Kogel.Slave.Mysql
{
    class JsonType : IDataType
    {
        public object ReadValue(ref SequenceReader<byte> reader, int meta)
        {
            int blobLength = reader.ReadInteger(meta);

            try
            {
                return reader.Sequence.Slice(reader.Consumed, blobLength).ToArray();
            }
            finally
            {
                reader.Advance(blobLength);
            }
        }
    }
}
