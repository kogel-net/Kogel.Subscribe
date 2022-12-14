using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Buffers;
using Kogel.Slave.Mysql.Extension;

namespace Kogel.Slave.Mysql
{
    internal interface IMySQLDataType
    {
        object ReadValue(ref SequenceReader<byte> reader, int meta);
    }
}
