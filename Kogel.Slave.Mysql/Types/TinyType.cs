using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Buffers;

namespace Kogel.Slave.Mysql
{
    class TinyType : IMySQLDataType
    {
        public object ReadValue(ref SequenceReader<byte> reader, int meta)
        {
            reader.TryRead(out byte x);
            return x;
        }
    }
}
