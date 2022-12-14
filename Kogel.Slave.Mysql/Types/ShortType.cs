using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Buffers;

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
