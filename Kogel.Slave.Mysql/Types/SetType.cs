using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Buffers;

namespace Kogel.Slave.Mysql
{
    class SetType : IMySQLDataType
    {
        public object ReadValue(ref SequenceReader<byte> reader, int meta)
        {
            return reader.ReadLong(4);
        }
    }
}
