using System;
using System.Buffers;
using System.Text;

namespace SuperSocket.ProtoBase
{

	public static class Extensions
	{
		public static string ReadString(this ref SequenceReader<byte> reader, long length = 0L)
		{
			return reader.ReadString(Encoding.UTF8, length);
		}

		public static string ReadString(this ref SequenceReader<byte> reader, Encoding encoding, long length = 0L)
		{
			if (length == 0L)
			{
				length = reader.Remaining;
			}
			ReadOnlySequence<byte> buffer = reader.Sequence.Slice(reader.Consumed, length);
			try
			{
				return buffer.GetString(encoding);
			}
			finally
			{
				reader.Advance(length);
			}
		}

		public static bool TryReadBigEndian(this ref SequenceReader<byte> reader, out ushort value)
		{
			value = 0;
			if (reader.Remaining < 2)
			{
				return false;
			}
			if (!reader.TryRead(out var value2))
			{
				return false;
			}
			if (!reader.TryRead(out var value3))
			{
				return false;
			}
			value = (ushort)(value2 * 256 + value3);
			return true;
		}

		public static bool TryReadBigEndian(this ref SequenceReader<byte> reader, out uint value)
		{
			value = 0u;
			if (reader.Remaining < 4)
			{
				return false;
			}
			int num = 0;
			int num2 = (int)Math.Pow(256.0, 3.0);
			for (int i = 0; i < 4; i++)
			{
				if (!reader.TryRead(out var value2))
				{
					return false;
				}
				num += num2 * value2;
				num2 /= 256;
			}
			value = (uint)num;
			return true;
		}

		public static bool TryReadBigEndian(this ref SequenceReader<byte> reader, out ulong value)
		{
			value = 0uL;
			if (reader.Remaining < 8)
			{
				return false;
			}
			long num = 0L;
			long num2 = (long)Math.Pow(256.0, 7.0);
			for (int i = 0; i < 8; i++)
			{
				if (!reader.TryRead(out var value2))
				{
					return false;
				}
				num += num2 * value2;
				num2 /= 256;
			}
			value = (ulong)num;
			return true;
		}

		public static string GetString(this ReadOnlySequence<byte> buffer, Encoding encoding)
		{
			if (buffer.IsSingleSegment)
			{
				return encoding.GetString(buffer.First.Span);
			}
			if (encoding.IsSingleByte)
			{
				return string.Create((int)buffer.Length, buffer, delegate (Span<char> span, ReadOnlySequence<byte> sequence)
				{
					ReadOnlySequence<byte>.Enumerator enumerator2 = sequence.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						ReadOnlyMemory<byte> current2 = enumerator2.Current;
						int chars2 = encoding.GetChars(current2.Span, span);
						span = span.Slice(chars2);
					}
				});
			}
			StringBuilder stringBuilder = new StringBuilder();
			Decoder decoder = encoding.GetDecoder();
			ReadOnlySequence<byte>.Enumerator enumerator = buffer.GetEnumerator();
			while (enumerator.MoveNext())
			{
				ReadOnlyMemory<byte> current = enumerator.Current;
				Span<char> span2 = new char[current.Length].AsSpan();
				int chars = decoder.GetChars(current.Span, span2, flush: false);
				stringBuilder.Append(new string((chars == span2.Length) ? span2 : span2.Slice(0, chars)));
			}
			return stringBuilder.ToString();
		}

		public static int Write(this IBufferWriter<byte> writer, ReadOnlySpan<char> text, Encoding encoding)
		{
			Encoder encoder = encoding.GetEncoder();
			bool completed = false;
			int num = 0;
			int maxByteCount = encoding.GetMaxByteCount(1);
			while (!completed)
			{
				Span<byte> span = writer.GetSpan(maxByteCount);
				encoder.Convert(text, span, flush: false, out var charsUsed, out var bytesUsed, out completed);
				if (charsUsed > 0)
				{
					text = text.Slice(charsUsed);
				}
				num += bytesUsed;
				writer.Advance(bytesUsed);
			}
			return num;
		}
	}
}