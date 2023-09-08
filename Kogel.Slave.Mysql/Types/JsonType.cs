using System.Buffers;
using Kogel.Slave.Mysql.Extensions;

namespace Kogel.Slave.Mysql
{
    class JsonType : IDataType
    {
        public object ReadValue(ref SequenceReader<byte> reader, int meta)
        {
            int jsonLength = reader.ReadInteger(meta);

            try
            {
                var jsons = reader.Sequence.Slice(reader.Consumed, jsonLength).ToArray();
                return jsons;
            }
            finally
            {
                reader.Advance(jsonLength);
            }
        }
    }
}
