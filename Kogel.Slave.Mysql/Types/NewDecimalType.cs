using Kogel.Slave.Mysql.Extensions;
using System;
using System.Buffers;
using System.Text;

namespace Kogel.Slave.Mysql
{
    class NewDecimalType : IDataType
    {
        public object ReadValue(ref SequenceReader<byte> reader, int meta)
        {
            if (meta < 8)
            {
                throw new Exception("不支持decimal长度小于8的字段类型(decimal(8))");
            }
            //整数长度
            int integerLength = 0;
            //小数长度
            int decimalLength = 0;
            if (meta < 256)
            {
                integerLength = meta;
            }
            else
            {
                integerLength = meta % 256;
                decimalLength = (meta - integerLength) / 256;
            }
            //前面的长度包括了小数位的长度
            int integerByteLength = GetLength(integerLength - decimalLength);
            int decimalByteLength = GetLength(decimalLength);

            reader.ReadInteger(1);
            string valueStr = $"{GetValue(ref reader, integerByteLength)}.{GetValue(ref reader, decimalByteLength)}";
            return Convert.ToDecimal(valueStr);
        }

        private int GetLength(int length, int number = 9)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                builder.Append(number);
            }
            double maxValue = Convert.ToDouble(builder.ToString());
            return (int)Math.Ceiling(Math.Log(maxValue, 256));
        }

        private long GetValue(ref SequenceReader<byte> reader, int length)
        {
            byte[] bytes = reader.ReadByteArray(length);
            long value = 0;
            for (int i = 0; i < length; i++)
            {
                long itemValue = i == length - 1 ? bytes[i] : (long)(bytes[i] * Math.Pow(256, length - i - 1));
                value += itemValue;
            }
            return value;
        }
    }
}
