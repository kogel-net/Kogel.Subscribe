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
            var version = EnvironmentExtensions.GetVersionEnvironmentVariable();
            if (version == Version.FivePlus)
            {
                return reader.ReadLengthEncodedString();
            }
            else
            {
                var length = reader.ReadInteger(1);
                string value = reader.ReadString(length, isExcludeZero: true);
                return value;
            }
        }
    }
}
