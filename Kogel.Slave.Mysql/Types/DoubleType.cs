using System;
using System.Buffers;
using Kogel.Slave.Mysql.Extensions;

namespace Kogel.Slave.Mysql
{
    class DoubleType : IDataType
    {
        public object ReadValue(ref SequenceReader<byte> reader, int meta)
        {
            byte[] bytes = reader.ReadByteArray(8);
            return BitConverter.ToDouble(bytes, 0);
        }
    }
}
