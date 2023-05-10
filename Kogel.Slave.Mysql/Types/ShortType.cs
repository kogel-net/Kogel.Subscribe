using System.Buffers;
using Kogel.Slave.Mysql.Extensions;

namespace Kogel.Slave.Mysql
{
    class ShortType : IDataType
    {
        public object ReadValue(ref SequenceReader<byte> reader, int meta)
        {
            return (short)reader.ReadInteger(2);
        }
    }
}
