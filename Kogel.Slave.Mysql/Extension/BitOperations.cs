using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static System.Buffers.SequenceReaderExtension;

namespace Kogel.Slave.Mysql.Extension
{
	public static class BitOperations
	{
		private static ReadOnlySpan<byte> s_TrailingZeroCountDeBruijn => new byte[32]
		{
		0, 1, 28, 2, 29, 14, 24, 3, 30, 22,
		20, 15, 25, 17, 4, 8, 31, 27, 13, 23,
		21, 19, 16, 7, 26, 12, 18, 6, 11, 5,
		10, 9
		};

		private static ReadOnlySpan<byte> s_Log2DeBruijn => new byte[32]
		{
		0, 9, 1, 10, 13, 21, 2, 29, 11, 14,
		16, 18, 22, 25, 3, 30, 8, 12, 20, 28,
		15, 17, 24, 7, 19, 27, 23, 6, 26, 5,
		4, 31
		};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static int LeadingZeroCount(uint value)
		{
			if (Lzcnt.IsSupported)
			{
				return (int)Lzcnt.LeadingZeroCount(value);
			}
			if (value == 0)
			{
				return 32;
			}
			return 31 - Log2SoftwareFallback(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static int LeadingZeroCount(ulong value)
		{
			if (Lzcnt.X64.IsSupported)
			{
				return (int)Lzcnt.X64.LeadingZeroCount(value);
			}
			uint num = (uint)(value >> 32);
			if (num == 0)
			{
				return 32 + LeadingZeroCount((uint)value);
			}
			return LeadingZeroCount(num);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static int Log2(uint value)
		{
			if (Lzcnt.IsSupported)
			{
				if (value == 0)
				{
					return 0;
				}
				return (int)(31 - Lzcnt.LeadingZeroCount(value));
			}
			return Log2SoftwareFallback(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static int Log2(ulong value)
		{
			if (Lzcnt.X64.IsSupported)
			{
				if (value == 0L)
				{
					return 0;
				}
				return 63 - (int)Lzcnt.X64.LeadingZeroCount(value);
			}
			uint num = (uint)(value >> 32);
			if (num == 0)
			{
				return Log2((uint)value);
			}
			return 32 + Log2(num);
		}

		private static int Log2SoftwareFallback(uint value)
		{
			value |= value >> 1;
			value |= value >> 2;
			value |= value >> 4;
			value |= value >> 8;
			value |= value >> 16;
			return Unsafe.AddByteOffset(ref MemoryMarshal.GetReference(s_Log2DeBruijn), (IntPtr)(int)(value * 130329821 >> 27));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int TrailingZeroCount(int value)
		{
			return TrailingZeroCount((uint)value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static int TrailingZeroCount(uint value)
		{
			if (Bmi1.IsSupported)
			{
				return (int)Bmi1.TrailingZeroCount(value);
			}
			if (value == 0)
			{
				return 32;
			}
			return Unsafe.AddByteOffset(ref MemoryMarshal.GetReference(s_TrailingZeroCountDeBruijn), (IntPtr)(int)((value & (0 - value)) * 125613361 >> 27));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int TrailingZeroCount(long value)
		{
			return TrailingZeroCount((ulong)value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static int TrailingZeroCount(ulong value)
		{
			if (Bmi1.X64.IsSupported)
			{
				return (int)Bmi1.X64.TrailingZeroCount(value);
			}
			uint num = (uint)value;
			if (num == 0)
			{
				return 32 + TrailingZeroCount((uint)(value >> 32));
			}
			return TrailingZeroCount(num);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static uint RotateLeft(uint value, int offset)
		{
			return (value << offset) | (value >> 32 - offset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static ulong RotateLeft(ulong value, int offset)
		{
			return (value << offset) | (value >> 64 - offset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static uint RotateRight(uint value, int offset)
		{
			return (value >> offset) | (value << 32 - offset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static ulong RotateRight(ulong value, int offset)
		{
			return (value >> offset) | (value << 64 - offset);
		}
	}

	[Intrinsic]
	[CLSCompliant(false)]
	public abstract class Lzcnt
	{
		[Intrinsic]
		public abstract class X64
		{
			public static bool IsSupported => IsSupported;

			public static ulong LeadingZeroCount(ulong value)
			{
				return LeadingZeroCount(value);
			}
		}

		public static bool IsSupported => IsSupported;

		public static uint LeadingZeroCount(uint value)
		{
			return LeadingZeroCount(value);
		}
	}

	[CLSCompliant(false)]
	[Intrinsic]
	public abstract class Bmi1
	{
		[Intrinsic]
		public abstract class X64
		{
			public static bool IsSupported => IsSupported;

			public static ulong AndNot(ulong left, ulong right)
			{
				return AndNot(left, right);
			}

			public static ulong BitFieldExtract(ulong value, byte start, byte length)
			{
				return BitFieldExtract(value, (ushort)(start | (length << 8)));
			}

			public static ulong BitFieldExtract(ulong value, ushort control)
			{
				return BitFieldExtract(value, control);
			}

			public static ulong ExtractLowestSetBit(ulong value)
			{
				return ExtractLowestSetBit(value);
			}

			public static ulong GetMaskUpToLowestSetBit(ulong value)
			{
				return GetMaskUpToLowestSetBit(value);
			}

			public static ulong ResetLowestSetBit(ulong value)
			{
				return ResetLowestSetBit(value);
			}

			public static ulong TrailingZeroCount(ulong value)
			{
				return TrailingZeroCount(value);
			}
		}

		public static bool IsSupported => IsSupported;

		public static uint AndNot(uint left, uint right)
		{
			return AndNot(left, right);
		}

		public static uint BitFieldExtract(uint value, byte start, byte length)
		{
			return BitFieldExtract(value, (ushort)(start | (length << 8)));
		}

		public static uint BitFieldExtract(uint value, ushort control)
		{
			return BitFieldExtract(value, control);
		}

		public static uint ExtractLowestSetBit(uint value)
		{
			return ExtractLowestSetBit(value);
		}

		public static uint GetMaskUpToLowestSetBit(uint value)
		{
			return GetMaskUpToLowestSetBit(value);
		}

		public static uint ResetLowestSetBit(uint value)
		{
			return ResetLowestSetBit(value);
		}

		public static uint TrailingZeroCount(uint value)
		{
			return TrailingZeroCount(value);
		}
	}

}
