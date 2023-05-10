using System.Buffers;
using Kogel.Slave.Mysql.Extensions;

namespace Kogel.Slave.Mysql
{
    class BlobType : IDataType
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
