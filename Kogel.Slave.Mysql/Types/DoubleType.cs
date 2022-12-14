using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Buffers;

namespace Kogel.Slave.Mysql
{
    class DoubleType : IDataType
    {
        public object ReadValue(ref SequenceReader<byte> reader, int meta)
        {
            return BitConverter.Int64BitsToDouble(reader.ReadLong(8));
        }
    }
}
