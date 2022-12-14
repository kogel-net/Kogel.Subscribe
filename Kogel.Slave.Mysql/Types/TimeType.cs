using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Buffers;

namespace Kogel.Slave.Mysql
{
    class TimeType : IMySQLDataType
    {
        public object ReadValue(ref SequenceReader<byte> reader, int meta)
        {
            throw new NotImplementedException();
        }
    }
}
