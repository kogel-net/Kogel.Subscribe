using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Buffers;
using Kogel.Slave.Mysql.Interface;

namespace Kogel.Slave.Mysql.Extension.DataType
{
    class DateTimeV2Type : TimeBaseType, IDataType
    {
        public object ReadValue(ref SequenceReader<byte> reader, int meta)
        {
            reader.TryReadBigEndian(out int totalValue0);
            reader.TryRead(out byte totalValue1);

            var totalValue = (long)totalValue0 * 256 + totalValue1;

            if (totalValue == 0)
                return null;

            var seconds = (int)(totalValue & 0x3F);

            totalValue = totalValue >> 6;
            var minutes = (int)(totalValue & 0x3F);

            totalValue = totalValue >> 6;   
            var hours = (int)(totalValue & 0x1F);

            totalValue = totalValue >> 5;
            var days = (int)(totalValue & 0x1F);

            totalValue = totalValue >> 5;
            var yearMonths = (int)(totalValue & 0x01FFFF);

            var year = yearMonths / 13;
            var month = yearMonths % 13;
            var fsp = ReadFractionalSeconds(ref reader, meta);

            return new DateTime(year, month, days, hours, minutes, seconds, fsp / 1000);     
        }
    }
}
