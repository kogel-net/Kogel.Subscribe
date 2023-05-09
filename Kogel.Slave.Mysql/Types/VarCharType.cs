using System.Buffers;
using Kogel.Slave.Mysql.Extensions;
using System.Text;
using System;

namespace Kogel.Slave.Mysql
{
    class VarCharType : IDataType
    {
        public object ReadValue(ref SequenceReader<byte> reader, int meta)
        {
            if (SlaveEnvironment.GetVersionEnvironmentVariable() == Version.EightPlus)
            {
                var length = reader.ReadInteger(meta);
                string value = reader.ReadString(length, isExcludeZero: true);
                return value;       
            }
            else
            {
                return reader.ReadLengthEncodedString();
            }
        }
    }
}
