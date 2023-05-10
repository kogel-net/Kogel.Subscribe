using System.Buffers;

namespace Kogel.Slave.Mysql
{
    internal interface IDataType
    {
        object ReadValue(ref SequenceReader<byte> reader, int meta);
    }
}
