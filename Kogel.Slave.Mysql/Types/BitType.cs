using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Buffers;
using Kogel.Slave.Mysql.Extensions;

namespace Kogel.Slave.Mysql
{
    class BitType : IDataType
    {
        public object ReadValue(ref SequenceReader<byte> reader, int meta)
        {
            //int bitArrayLength = (meta >> 8) * 8 + (meta & 0xFF);
            //return reader.ReadBitArray(bitArrayLength, false);

            int bitLength= (meta >> 8) * 8 + (meta & 0xFF);
            return reader.ReadInteger(bitLength);
        }
    }
}
