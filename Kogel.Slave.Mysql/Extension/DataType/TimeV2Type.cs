using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Buffers;
using Kogel.Slave.Mysql.Interface;

namespace Kogel.Slave.Mysql.Extension.DataType
{
    class TimeV2Type : IDataType
    {
        public object ReadValue(ref SequenceReader<byte> reader, int meta)
        {
            throw new NotImplementedException();
        }
    }
}
