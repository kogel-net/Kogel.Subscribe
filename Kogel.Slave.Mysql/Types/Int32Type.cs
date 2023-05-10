using System;
using System.Buffers;
using Kogel.Slave.Mysql.Extensions;

namespace Kogel.Slave.Mysql
{
    class Int32Type : IDataType
    {
        public object ReadValue(ref SequenceReader<byte> reader, int meta)
        {
            byte[] bytes = reader.ReadByteArray(4);
            return BitConverter.ToInt32(bytes, 0);
        }
    }
}
