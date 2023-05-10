using System;
using System.Buffers;
using Kogel.Slave.Mysql.Extensions;

namespace Kogel.Slave.Mysql
{
    class NewDecimalType : IDataType
    {
        public object ReadValue(ref SequenceReader<byte> reader, int meta)
        {
            /*byte[] decimalBytes = reader.ReadByteArray(16);

            int[] bits = new int[4];

            // Copy the bytes to the bits array in little-endian order
            Buffer.BlockCopy(decimalBytes, 0, bits, 0, 16);

            // Create a decimal value from the bits array
            decimal value = new decimal(bits);
            return value;*/
            byte[] bytes = reader.ReadByteArray(8);
            var value= BitConverter.ToDouble(bytes, 0);
            return value;
            //return decimal.Parse(reader.ReadLengthEncodedString(), CultureInfo.InvariantCulture);
        }

        private decimal Parse(byte [] bytes)
        {  
            int[] bits = new int[4];
            bits[0] = (bytes[0] & 0x7F) | ((bytes[0] & 0x80) << 1);
            bits[1] = bytes[1];
            bits[2] = bytes[2];
            bits[3] = bytes[3];
            decimal decimalValue = new decimal(bits);
            if ((bytes[0] & 0x80) != 0)
            {
                decimalValue = decimal.Negate(decimalValue);
                return decimalValue;
            }
            return 0;
        }
    }
}
