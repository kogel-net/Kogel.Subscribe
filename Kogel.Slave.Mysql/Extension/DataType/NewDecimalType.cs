using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Buffers;
using System.Globalization;
using Kogel.Slave.Mysql.Interface;

namespace Kogel.Slave.Mysql.Extension.DataType
{
    class NewDecimalType : IDataType
    {
        public object ReadValue(ref SequenceReader<byte> reader, int meta)
        {
            return decimal.Parse(reader.ReadLengthEncodedString(), CultureInfo.InvariantCulture);
        }
    }
}
