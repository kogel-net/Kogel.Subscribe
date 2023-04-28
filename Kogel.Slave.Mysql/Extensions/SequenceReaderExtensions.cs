using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;

namespace Kogel.Slave.Mysql.Extensions
{
    public static class SequenceReaderExtensions
    {
        internal static BitArray ReadBitArray(ref this SequenceReader<byte> reader, int length, bool bigEndian = false)
        {
            var dataLen = (length + 7) / 8;
            var array = new BitArray(length, false);

            if (!bigEndian)
            {
                for (int i = 0; i < dataLen; i++)
                {
                    reader.TryRead(out byte b);
                    SetBitArray(array, b, i, length);
                }
            }
            else
            {
                for (int i = dataLen - 1; i >= 0; i--)
                {
                    reader.TryRead(out byte b);
                    SetBitArray(array, b, i, length);
                }
            }

            return array;
        }

        private static void SetBitArray(BitArray array, byte b, int i, int length)
        {
            for (var j = i * 8; j < Math.Min((i + 1) * 8, length); j++)
            {
                if ((b & 0x01 << j % 8) != 0x00)
                    array.Set(j, true);
            }
        }

        internal static string ReadString(ref this SequenceReader<byte> reader, Encoding encoding)
        {
            return reader.ReadString(encoding, out long consumed);
        }

        internal static string ReadString(ref this SequenceReader<byte> reader, Encoding encoding, out long consumed)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;

            if (reader.TryReadTo(out ReadOnlySequence<byte> seq, 0x00, false))
            {
                consumed = seq.Length + 1;
                var result = seq.GetString(encoding);
                reader.Advance(1);
                return result;
            }
            else
            {
                consumed = reader.Remaining;
                seq = reader.Sequence;
                seq = seq.Slice(reader.Consumed);
                var result = seq.GetString(Encoding.UTF8);
                reader.Advance(consumed);
                return result;
            }
        }

        internal static string ReadString(ref this SequenceReader<byte> reader, long length = 0, bool isExcludeZero = false)
        {
            return reader.ReadString(Encoding.UTF8, length, isExcludeZero);
        }

        internal static string ReadString(ref this SequenceReader<byte> reader, Encoding encoding, long length = 0, bool isExcludeZero = false)
        {
            if (length == 0 || reader.Remaining <= length)
                return reader.ReadString(encoding);

            // reader.Remaining > length
            var sequence = reader.Sequence.Slice(reader.Consumed, length);
            try
            {
                if (isExcludeZero)
                {
                    sequence = ReadOnlySequenceExcludeZero(ref reader, sequence, ref length);
                }
                var subReader = new SequenceReader<byte>(sequence);
                return subReader.ReadString(encoding, out long consumed);
            }
            finally
            {
                reader.Advance(length);
            }
        }

        /// <summary>
        /// 可能存在0的空值，排除0并往后偏移拿值
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="sequence"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        internal static ReadOnlySequence<byte> ReadOnlySequenceExcludeZero(ref this SequenceReader<byte> reader, ReadOnlySequence<byte> sequence, ref long length)
        {
            var bytArr = sequence.ToArray();
            var newByts = bytArr.Where(x => x != 0).ToList();
            var spliceSeq = reader.Sequence.Slice(reader.Consumed + length, length - newByts.Count);
            length += length - newByts.Count;
            newByts.AddRange(spliceSeq.ToArray());
            return new ReadOnlySequence<byte>(newByts.ToArray());
        }

        internal static int ReadInteger(ref this SequenceReader<byte> reader, int length)
        {
            if (length > 4)
                throw new ArgumentException("Length cannot be more than 4.", nameof(length));

            var unit = 1;
            var value = 0;

            for (var i = 0; i < length; i++)
            {
                reader.TryRead(out byte thisValue);
                value += thisValue * unit;
                unit *= 256;
            }

            return value;
        }

        internal static int ReadBigEndianInteger(ref this SequenceReader<byte> reader, int length)
        {
            if (length > 4)
                throw new ArgumentException("Length cannot be more than 4.", nameof(length));

            var unit = (int)Math.Pow(256, length - 1);
            var value = 0;

            for (var i = 0; i < length; i++)
            {
                reader.TryRead(out byte thisValue);
                value += thisValue * (int)Math.Pow(256, length - i - 1);
            }

            return value;
        }

        internal static long ReadLong(ref this SequenceReader<byte> reader, int length)
        {
            var unit = 1;
            var value = 0L;

            for (var i = 0; i < length; i++)
            {
                reader.TryRead(out byte thisValue);
                value += thisValue * unit;
                unit *= 256;
            }

            return value;
        }

        internal static string ReadLengthEncodedString(ref this SequenceReader<byte> reader)
        {
            return reader.ReadLengthEncodedString(Encoding.UTF8);
        }

        internal static string ReadLengthEncodedString(ref this SequenceReader<byte> reader, Encoding encoding)
        {
            var len = reader.ReadLengthEncodedInteger();

            if (len < 0)
                return null;

            if (len == 0)
                return string.Empty;

            return reader.ReadString(encoding, len);
        }

        internal static long ReadLengthEncodedInteger(ref this SequenceReader<byte> reader)
        {
            reader.TryRead(out byte b0);

            if (b0 == 0xFB) // 251
                return -1;

            if (b0 == 0xFC) // 252
            {
                reader.TryReadLittleEndian(out short shortValue);
                return shortValue;
            }

            if (b0 == 0xFD) // 253
            {
                reader.TryRead(out byte b1);
                reader.TryRead(out byte b2);
                reader.TryRead(out byte b3);

                return b1 + b2 * 256 + b3 * 256 * 256;
            }

            if (b0 == 0xFE) // 254
            {
                reader.TryReadLittleEndian(out long longValue);
                return longValue;
            }

            return b0;
        }
    }
}
