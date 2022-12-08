using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Buffers;
using Kogel.Slave.Mysql.Interface;

namespace Kogel.Slave.Mysql.Extension.DataType
{
    class TimestampV2Type : TimeBaseType, IDataType
    {
        public object ReadValue(ref SequenceReader<byte> reader, int meta)
        {
            var millis = (long)reader.ReadBigEndianInteger(4);
            var fsp = ReadFractionalSeconds(ref reader, meta);
            var ticks = millis * 1000 + fsp / 1000;
            return new DateTime(ticks);
        }
    }
}
