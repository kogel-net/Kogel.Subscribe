using System;
using System.Buffers;
using Kogel.Slave.Mysql.Extensions;

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
