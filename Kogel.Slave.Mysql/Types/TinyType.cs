using System.Buffers;

namespace Kogel.Slave.Mysql
{
    class TinyType : IDataType
    {
        public object ReadValue(ref SequenceReader<byte> reader, int meta)
        {
            reader.TryRead(out byte x);
            return x;
        }
    }
}
