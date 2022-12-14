using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Buffers
{
    /// <summary>
    /// 
    /// </summary>
    public static class SequenceReaderExtension
    {
        internal static int ReadInteger(this ref SequenceReader<byte> reader, int length)
        {
            if (length > 4)
            {
                throw new ArgumentException("Length cannot be more than 4.", "length");
            }
            int unit = 1;
            int value = 0;
            for (int i = 0; i < length; i++)
            {
                reader.TryRead(out var thisValue);
                value += thisValue * unit;
                unit *= 256;
            }
            return value;
        }

        internal static long ReadLong(this ref SequenceReader<byte> reader, int length)
        {
            int unit = 1;
            long value = 0L;
            for (int i = 0; i < length; i++)
            {
                reader.TryRead(out var thisValue);
                value += thisValue * unit;
                unit *= 256;
            }
            return value;
        }

        internal static string ReadLengthEncodedString(this ref SequenceReader<byte> reader)
        {
            return reader.ReadLengthEncodedString(Encoding.UTF8);
        }

        internal static string ReadLengthEncodedString(this ref SequenceReader<byte> reader, Encoding encoding)
        {
            long len = reader.ReadLengthEncodedInteger();
            if (len < 0)
            {
                return null;
            }
            if (len == 0)
            {
                return string.Empty;
            }
            return reader.ReadString(encoding, len);
        }

        internal static long ReadLengthEncodedInteger(this ref SequenceReader<byte> reader)
        {
            reader.TryRead(out var b0);
            switch (b0)
            {
                case 251:
                    return -1L;
                case 252:
                    {
                        reader.TryReadLittleEndian(out short shortValue);
                        return shortValue;
                    }
                case 253:
                    {
                        reader.TryRead(out var b1);
                        reader.TryRead(out var b2);
                        reader.TryRead(out var b3);
                        return b1 + b2 * 256 + b3 * 256 * 256;
                    }
                case 254:
                    {
                        reader.TryReadLittleEndian(out short longValue);
                        return longValue;
                    }
                default:
                    return b0;
            }
        }

        internal static string ReadString(this ref SequenceReader<byte> reader, Encoding encoding, long length = 0L)
        {
            if (length == 0L || reader.Remaining <= length)
            {
                return reader.ReadString(encoding);
            }
            ReadOnlySequence<byte> seq = reader.Sequence.Slice(reader.Consumed, length);
            long consumed = 0L;
            try
            {
                SequenceReader<byte> subReader = new SequenceReader<byte>(seq);
                //out consumed
                return subReader.ReadString(encoding,  consumed);
            }
            finally
            {
                reader.Advance(length);
            }
        }

#if NETSTANDARD2_0
        public static bool TryReadLittleEndian(this ref SequenceReader<byte> reader, out short value)
        {
            if (BitConverter.IsLittleEndian)
            {
                return reader.TryRead<short>(out value);
            }
            return TryReadReverseEndianness(ref reader, out value);
        }
#endif

        private static bool TryReadReverseEndianness(ref SequenceReader<byte> reader, out short value)
        {
            if (reader.TryRead<short>(out value))
            {
                value = BinaryPrimitives.ReverseEndianness(value);
                return true;
            }
            return false;
        }


		internal static int ReadBigEndianInteger(this ref SequenceReader<byte> reader, int length)
		{
			if (length > 4)
			{
				throw new ArgumentException("Length cannot be more than 4.", "length");
			}
			int unit = (int)Math.Pow(256.0, length - 1);
			int value = 0;
			for (int i = 0; i < length; i++)
			{
				reader.TryRead(out var thisValue);
				value += thisValue * (int)Math.Pow(256.0, length - i - 1);
			}
			return value;
		}


		internal static BitArray ReadBitArray(this ref SequenceReader<byte> reader, int length, bool bigEndian = false)
		{
			int dataLen = (length + 7) / 8;
			BitArray array = new BitArray(length, defaultValue: false);
			if (!bigEndian)
			{
				for (int j = 0; j < dataLen; j++)
				{
					reader.TryRead(out var b2);
					SetBitArray(array, b2, j, length);
				}
			}
			else
			{
				for (int i = dataLen - 1; i >= 0; i--)
				{
					reader.TryRead(out var b);
					SetBitArray(array, b, i, length);
				}
			}
			return array;
		}

		private static void SetBitArray(BitArray array, byte b, int i, int length)
		{
			for (int j = i * 8; j < Math.Min((i + 1) * 8, length); j++)
			{
				if ((b & (1 << j % 8)) != 0)
				{
					array.Set(j, value: true);
				}
			}
		}

#if NETSTANDARD2_0
        public static bool TryReadBigEndian(this ref SequenceReader<byte> reader, out int value)
		{
			if (!BitConverter.IsLittleEndian)
			{
				return reader.TryRead<int>(out value);
			}
			
			var isfig= TryReadReverseEndianness(ref reader, out short svalue);
			value = svalue;
			return isfig;
		}
#endif

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Field, Inherited = false)]
		internal sealed class IntrinsicAttribute : Attribute
		{
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe static bool TryRead<T>(this ref SequenceReader<byte> reader, out T value) where T : unmanaged
        {
            ReadOnlySpan<byte> unreadSpan = reader.UnreadSpan;
            if (unreadSpan.Length < sizeof(T))
            {
                return TryReadMultisegment<T>(ref reader, out value);
            }
            value = Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(unreadSpan));
            reader.Advance(sizeof(T));
            return true;
        }

		private unsafe static bool TryReadMultisegment<T>(ref SequenceReader<byte> reader, out T value) where T : unmanaged
		{
			T val = default(T);
			Span<byte> span = new Span<byte>(&val, sizeof(T));
			if (!reader.TryCopyTo(span))
			{
				value = default(T);
				return false;
			}
			value = Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(span));
			reader.Advance(sizeof(T));
			return true;
		}

        internal static string ReadString(ref this SequenceReader<byte> reader, long length = 0)
        {
            return ReadString(ref reader, Encoding.UTF8, length);
        }
    }
}
