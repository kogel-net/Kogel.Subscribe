using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Buffers;
using Kogel.Slave.Mysql.Interface;

namespace Kogel.Slave.Mysql.Extension.DataType
{
    /// <summary>
    /// 
    /// </summary>
   public class BitType : IDataType
    {

        public object ReadValue(ref SequenceReader<byte> reader, int meta)
        {
            int bitArrayLength = (meta >> 8) * 8 + (meta & 0xFF);
            return reader.ReadBitArray(bitArrayLength, false);
        }
    }
}
