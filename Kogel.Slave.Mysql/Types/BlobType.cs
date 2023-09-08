using System.Buffers;
using System.Text;
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
                var blobs = reader.Sequence.Slice(reader.Consumed, blobLength).ToArray();
                var blobValue = Encoding.UTF8.GetString(blobs);
                return blobValue;
            }
            finally
            {
                reader.Advance(blobLength);
            }
        }
    }
}
