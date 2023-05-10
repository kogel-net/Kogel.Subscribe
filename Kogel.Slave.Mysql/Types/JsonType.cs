using System.Buffers;
using System.IO;
using System.Text;
using Kogel.Slave.Mysql.Extensions;
using LZ4;

namespace Kogel.Slave.Mysql
{
    class JsonType : IDataType
    {
        public object ReadValue(ref SequenceReader<byte> reader, int meta)
        {
            int length = reader.ReadInteger(meta);
            /*string json = reader.ReadString(length, false);
            return json;*/
            byte[] bytes = reader.ReadByteArray(length);
            string json = Encoding.UTF8.GetString(bytes);
            //byte[] unCompressionBytes = UnLZ4Compression(bytes);
            //var jsonString = Encoding.UTF8.GetString(unCompressionBytes);
            //Console.WriteLine(jsonString);
            return json;
        }

        private byte[] UnLZ4Compression(byte[] bytes)
        {
            using (MemoryStream compressedStream = new MemoryStream(bytes))
            {
                // 创建一个LZ4Stream对象，以便我们可以将其解压缩
                using (LZ4Stream lz4Stream = new LZ4Stream(compressedStream, LZ4StreamMode.Decompress))
                {
                    // 创建一个MemoryStream对象，以便我们可以从LZ4Stream中读取解压缩的数据
                    using (MemoryStream decompressedStream = new MemoryStream())
                    {
                        // 将解压缩的数据从LZ4Stream中读取到MemoryStream中
                        lz4Stream.CopyTo(decompressedStream);

                        // 获取解压缩后的数据
                        byte[] decompressedData = decompressedStream.ToArray();

                        return decompressedData;
                    }
                }
            }
        }
    }
}
