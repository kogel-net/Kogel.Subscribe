using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Buffers;
using Kogel.Slave.Mysql.Interface;

namespace Kogel.Slave.Mysql.Extension.DataType
{
    class DateType : IDataType
    {
        public object ReadValue(ref SequenceReader<byte> reader, int meta)
        {
            var value = reader.ReadInteger(3);
            var day = value % 32;
            value >>= 5;
            int month = value % 16;
            int year = value >> 4;

            return new DateTime(year, month, day, 0, 0, 0);
        }
    }
}
