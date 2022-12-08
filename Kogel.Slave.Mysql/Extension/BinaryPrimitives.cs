using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static Kogel.Slave.Mysql.Extension.SequenceReaderExtension;

namespace Kogel.Slave.Mysql.Extension
{
	public static class BinaryPrimitives
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static sbyte ReverseEndianness(sbyte value)
		{
			return value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Intrinsic]
		public static short ReverseEndianness(short value)
		{
			return (short)ReverseEndianness((ushort)value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Intrinsic]
		public static int ReverseEndianness(int value)
		{
			return (int)ReverseEndianness((uint)value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Intrinsic]
		public static long ReverseEndianness(long value)
		{
			return (long)ReverseEndianness((ulong)value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte ReverseEndianness(byte value)
		{
			return value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		[Intrinsic]
		public static ushort ReverseEndianness(ushort value)
		{
			return (ushort)((value >> 8) + (value << 8));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Intrinsic]
		[CLSCompliant(false)]
		public static uint ReverseEndianness(uint value)
		{
			return BitOperations.RotateRight(value & 0xFF00FFu, 8) + BitOperations.RotateLeft(value & 0xFF00FF00u, 8);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Intrinsic]
		[CLSCompliant(false)]
		public static ulong ReverseEndianness(ulong value)
		{
			return ((ulong)ReverseEndianness((uint)value) << 32) + ReverseEndianness((uint)(value >> 32));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static short ReadInt16BigEndian(ReadOnlySpan<byte> source)
		{
			short num = MemoryMarshal.Read<short>(source);
			if (BitConverter.IsLittleEndian)
			{
				num = ReverseEndianness(num);
			}
			return num;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int ReadInt32BigEndian(ReadOnlySpan<byte> source)
		{
			int num = MemoryMarshal.Read<int>(source);
			if (BitConverter.IsLittleEndian)
			{
				num = ReverseEndianness(num);
			}
			return num;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long ReadInt64BigEndian(ReadOnlySpan<byte> source)
		{
			long num = MemoryMarshal.Read<long>(source);
			if (BitConverter.IsLittleEndian)
			{
				num = ReverseEndianness(num);
			}
			return num;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static ushort ReadUInt16BigEndian(ReadOnlySpan<byte> source)
		{
			ushort num = MemoryMarshal.Read<ushort>(source);
			if (BitConverter.IsLittleEndian)
			{
				num = ReverseEndianness(num);
			}
			return num;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static uint ReadUInt32BigEndian(ReadOnlySpan<byte> source)
		{
			uint num = MemoryMarshal.Read<uint>(source);
			if (BitConverter.IsLittleEndian)
			{
				num = ReverseEndianness(num);
			}
			return num;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static ulong ReadUInt64BigEndian(ReadOnlySpan<byte> source)
		{
			ulong num = MemoryMarshal.Read<ulong>(source);
			if (BitConverter.IsLittleEndian)
			{
				num = ReverseEndianness(num);
			}
			return num;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryReadInt16BigEndian(ReadOnlySpan<byte> source, out short value)
		{
			bool result = MemoryMarshal.TryRead<short>(source, out value);
			if (BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryReadInt32BigEndian(ReadOnlySpan<byte> source, out int value)
		{
			bool result = MemoryMarshal.TryRead<int>(source, out value);
			if (BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryReadInt64BigEndian(ReadOnlySpan<byte> source, out long value)
		{
			bool result = MemoryMarshal.TryRead<long>(source, out value);
			if (BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static bool TryReadUInt16BigEndian(ReadOnlySpan<byte> source, out ushort value)
		{
			bool result = MemoryMarshal.TryRead<ushort>(source, out value);
			if (BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static bool TryReadUInt32BigEndian(ReadOnlySpan<byte> source, out uint value)
		{
			bool result = MemoryMarshal.TryRead<uint>(source, out value);
			if (BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static bool TryReadUInt64BigEndian(ReadOnlySpan<byte> source, out ulong value)
		{
			bool result = MemoryMarshal.TryRead<ulong>(source, out value);
			if (BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static short ReadInt16LittleEndian(ReadOnlySpan<byte> source)
		{
			short num = MemoryMarshal.Read<short>(source);
			if (!BitConverter.IsLittleEndian)
			{
				num = ReverseEndianness(num);
			}
			return num;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int ReadInt32LittleEndian(ReadOnlySpan<byte> source)
		{
			int num = MemoryMarshal.Read<int>(source);
			if (!BitConverter.IsLittleEndian)
			{
				num = ReverseEndianness(num);
			}
			return num;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long ReadInt64LittleEndian(ReadOnlySpan<byte> source)
		{
			long num = MemoryMarshal.Read<long>(source);
			if (!BitConverter.IsLittleEndian)
			{
				num = ReverseEndianness(num);
			}
			return num;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static ushort ReadUInt16LittleEndian(ReadOnlySpan<byte> source)
		{
			ushort num = MemoryMarshal.Read<ushort>(source);
			if (!BitConverter.IsLittleEndian)
			{
				num = ReverseEndianness(num);
			}
			return num;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static uint ReadUInt32LittleEndian(ReadOnlySpan<byte> source)
		{
			uint num = MemoryMarshal.Read<uint>(source);
			if (!BitConverter.IsLittleEndian)
			{
				num = ReverseEndianness(num);
			}
			return num;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static ulong ReadUInt64LittleEndian(ReadOnlySpan<byte> source)
		{
			ulong num = MemoryMarshal.Read<ulong>(source);
			if (!BitConverter.IsLittleEndian)
			{
				num = ReverseEndianness(num);
			}
			return num;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryReadInt16LittleEndian(ReadOnlySpan<byte> source, out short value)
		{
			bool result = MemoryMarshal.TryRead<short>(source, out value);
			if (!BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryReadInt32LittleEndian(ReadOnlySpan<byte> source, out int value)
		{
			bool result = MemoryMarshal.TryRead<int>(source, out value);
			if (!BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryReadInt64LittleEndian(ReadOnlySpan<byte> source, out long value)
		{
			bool result = MemoryMarshal.TryRead<long>(source, out value);
			if (!BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static bool TryReadUInt16LittleEndian(ReadOnlySpan<byte> source, out ushort value)
		{
			bool result = MemoryMarshal.TryRead<ushort>(source, out value);
			if (!BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static bool TryReadUInt32LittleEndian(ReadOnlySpan<byte> source, out uint value)
		{
			bool result = MemoryMarshal.TryRead<uint>(source, out value);
			if (!BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static bool TryReadUInt64LittleEndian(ReadOnlySpan<byte> source, out ulong value)
		{
			bool result = MemoryMarshal.TryRead<ulong>(source, out value);
			if (!BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteInt16BigEndian(Span<byte> destination, short value)
		{
			if (BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			MemoryMarshal.Write(destination, ref value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteInt32BigEndian(Span<byte> destination, int value)
		{
			if (BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			MemoryMarshal.Write(destination, ref value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteInt64BigEndian(Span<byte> destination, long value)
		{
			if (BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			MemoryMarshal.Write(destination, ref value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static void WriteUInt16BigEndian(Span<byte> destination, ushort value)
		{
			if (BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			MemoryMarshal.Write(destination, ref value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static void WriteUInt32BigEndian(Span<byte> destination, uint value)
		{
			if (BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			MemoryMarshal.Write(destination, ref value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static void WriteUInt64BigEndian(Span<byte> destination, ulong value)
		{
			if (BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			MemoryMarshal.Write(destination, ref value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryWriteInt16BigEndian(Span<byte> destination, short value)
		{
			if (BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			return MemoryMarshal.TryWrite(destination, ref value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryWriteInt32BigEndian(Span<byte> destination, int value)
		{
			if (BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			return MemoryMarshal.TryWrite(destination, ref value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryWriteInt64BigEndian(Span<byte> destination, long value)
		{
			if (BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			return MemoryMarshal.TryWrite(destination, ref value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static bool TryWriteUInt16BigEndian(Span<byte> destination, ushort value)
		{
			if (BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			return MemoryMarshal.TryWrite(destination, ref value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static bool TryWriteUInt32BigEndian(Span<byte> destination, uint value)
		{
			if (BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			return MemoryMarshal.TryWrite(destination, ref value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static bool TryWriteUInt64BigEndian(Span<byte> destination, ulong value)
		{
			if (BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			return MemoryMarshal.TryWrite(destination, ref value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteInt16LittleEndian(Span<byte> destination, short value)
		{
			if (!BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			MemoryMarshal.Write(destination, ref value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteInt32LittleEndian(Span<byte> destination, int value)
		{
			if (!BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			MemoryMarshal.Write(destination, ref value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteInt64LittleEndian(Span<byte> destination, long value)
		{
			if (!BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			MemoryMarshal.Write(destination, ref value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static void WriteUInt16LittleEndian(Span<byte> destination, ushort value)
		{
			if (!BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			MemoryMarshal.Write(destination, ref value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static void WriteUInt32LittleEndian(Span<byte> destination, uint value)
		{
			if (!BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			MemoryMarshal.Write(destination, ref value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static void WriteUInt64LittleEndian(Span<byte> destination, ulong value)
		{
			if (!BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			MemoryMarshal.Write(destination, ref value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryWriteInt16LittleEndian(Span<byte> destination, short value)
		{
			if (!BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			return MemoryMarshal.TryWrite(destination, ref value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryWriteInt32LittleEndian(Span<byte> destination, int value)
		{
			if (!BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			return MemoryMarshal.TryWrite(destination, ref value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryWriteInt64LittleEndian(Span<byte> destination, long value)
		{
			if (!BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			return MemoryMarshal.TryWrite(destination, ref value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static bool TryWriteUInt16LittleEndian(Span<byte> destination, ushort value)
		{
			if (!BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			return MemoryMarshal.TryWrite(destination, ref value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static bool TryWriteUInt32LittleEndian(Span<byte> destination, uint value)
		{
			if (!BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			return MemoryMarshal.TryWrite(destination, ref value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static bool TryWriteUInt64LittleEndian(Span<byte> destination, ulong value)
		{
			if (!BitConverter.IsLittleEndian)
			{
				value = ReverseEndianness(value);
			}
			return MemoryMarshal.TryWrite(destination, ref value);
		}
	}
}
