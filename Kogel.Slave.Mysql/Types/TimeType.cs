using System;
using System.Buffers;

namespace Kogel.Slave.Mysql
{
    class TimeType : IDataType
    {
        public object ReadValue(ref SequenceReader<byte> reader, int meta)
        {
            throw new NotImplementedException();
        }
    }
}
