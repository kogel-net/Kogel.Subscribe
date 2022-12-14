using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Buffers;

namespace Kogel.Slave.Mysql
{
    class TimestampType : IDataType
    {
        public object ReadValue(ref SequenceReader<byte> reader, int meta)
        {
            return new DateTime(reader.ReadLong(4) * 1000);
        }
    }
}
