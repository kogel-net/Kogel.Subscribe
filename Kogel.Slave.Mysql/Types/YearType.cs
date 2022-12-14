using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Buffers;

namespace Kogel.Slave.Mysql
{
    class YearType : IDataType
    {
        public object ReadValue(ref SequenceReader<byte> reader, int meta)
        {
            return 1900 + reader.ReadInteger(1);
        }
    }
}
