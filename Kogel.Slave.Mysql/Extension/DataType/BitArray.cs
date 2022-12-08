using System;
using System.Buffers.Binary;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Kogel.Slave.Mysql.Extension.DataType
{
	[Serializable]
	[TypeForwardedFrom("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
	public sealed class BitArray : ICollection, IEnumerable, ICloneable
	{
		private class BitArrayEnumeratorSimple : IEnumerator, ICloneable
		{
			private BitArray _bitarray;

			private int _index;

			private readonly int _version;

			private bool _currentElement;

			public virtual object Current
			{
				get
				{
					if ((uint)_index >= (uint)_bitarray.Count)
					{
						throw GetInvalidOperationException(_index);
					}
					return _currentElement;
				}
			}

			internal BitArrayEnumeratorSimple(BitArray bitarray)
			{
				_bitarray = bitarray;
				_index = -1;
				_version = bitarray._version;
			}

			public object Clone()
			{
				return MemberwiseClone();
			}

			public virtual bool MoveNext()
			{
				ICollection bitarray = _bitarray;
				if (_version != _bitarray._version)
				{
					throw new InvalidOperationException();
				}
				if (_index < bitarray.Count - 1)
				{
					_index++;
					_currentElement = _bitarray.Get(_index);
					return true;
				}
				_index = bitarray.Count;
				return false;
			}

			public void Reset()
			{
				if (_version != _bitarray._version)
				{
					throw new InvalidOperationException();
				}
				_index = -1;
			}

			private InvalidOperationException GetInvalidOperationException(int index)
			{
				if (index == -1)
				{
					return new InvalidOperationException();
				}
				return new InvalidOperationException();
			}
		}

		private int[] m_array;

		private int m_length;

		private int _version;

		public bool this[int index]
		{
			get
			{
				return Get(index);
			}
			set
			{
				Set(index, value);
			}
		}

		public int Length
		{
			get
			{
				return m_length;
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException();
				}
				int int32ArrayLengthFromBitLength = GetInt32ArrayLengthFromBitLength(value);
				if (int32ArrayLengthFromBitLength > m_array.Length || int32ArrayLengthFromBitLength + 256 < m_array.Length)
				{
					Array.Resize(ref m_array, int32ArrayLengthFromBitLength);
				}
				if (value > m_length)
				{
					int num = m_length - 1 >> 5;
					Div32Rem(m_length, out var remainder);
					if (remainder > 0)
					{
						m_array[num] &= (1 << remainder) - 1;
					}
					m_array.AsSpan(num + 1, int32ArrayLengthFromBitLength - num - 1).Clear();
				}
				m_length = value;
				_version++;
			}
		}

		public int Count => m_length;

		public object SyncRoot => this;

		public bool IsSynchronized => false;

		public bool IsReadOnly => false;

		public BitArray(int length)
			: this(length, defaultValue: false)
		{
		}

		public BitArray(int length, bool defaultValue)
		{
			if (length < 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			m_array = new int[GetInt32ArrayLengthFromBitLength(length)];
			m_length = length;
			if (defaultValue)
			{
				m_array.AsSpan().Fill(-1);
			}
			_version = 0;
		}

		public BitArray(byte[] bytes)
		{
			if (bytes == null)
			{
				throw new ArgumentNullException("bytes");
			}
			if (bytes.Length > 268435455)
			{
				throw new ArgumentException();
			}
			m_array = new int[GetInt32ArrayLengthFromByteLength(bytes.Length)];
			m_length = bytes.Length * 8;
			uint num = (uint)bytes.Length / 4u;
			ReadOnlySpan<byte> source = bytes;
			for (int i = 0; i < num; i++)
			{
				m_array[i] = BinaryPrimitives.ReadInt32LittleEndian(source);
				source = source.Slice(4);
			}
			int num2 = 0;
			switch (source.Length)
			{
				case 3:
					num2 = source[2] << 16;
					goto case 2;
				case 2:
					num2 |= source[1] << 8;
					goto case 1;
				case 1:
					m_array[num] = num2 | source[0];
					break;
			}
			_version = 0;
		}

		public BitArray(bool[] values)
		{
			if (values == null)
			{
				throw new ArgumentNullException("values");
			}
			m_array = new int[GetInt32ArrayLengthFromBitLength(values.Length)];
			m_length = values.Length;
			for (int i = 0; i < values.Length; i++)
			{
				if (values[i])
				{
					int remainder;
					int num = Div32Rem(i, out remainder);
					m_array[num] |= 1 << remainder;
				}
			}
			_version = 0;
		}

		public BitArray(int[] values)
		{
			if (values == null)
			{
				throw new ArgumentNullException("values");
			}
			if (values.Length > 67108863)
			{
				throw new ArgumentException();
			}
			m_array = new int[values.Length];
			Array.Copy(values, 0, m_array, 0, values.Length);
			m_length = values.Length * 32;
			_version = 0;
		}

		public BitArray(BitArray bits)
		{
			if (bits == null)
			{
				throw new ArgumentNullException("bits");
			}
			int int32ArrayLengthFromBitLength = GetInt32ArrayLengthFromBitLength(bits.m_length);
			m_array = new int[int32ArrayLengthFromBitLength];
			Array.Copy(bits.m_array, 0, m_array, 0, int32ArrayLengthFromBitLength);
			m_length = bits.m_length;
			_version = bits._version;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Get(int index)
		{
			if ((uint)index >= (uint)m_length)
			{
				ThrowArgumentOutOfRangeException(index);
			}
			return (m_array[index >> 5] & (1 << index)) != 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(int index, bool value)
		{
			if ((uint)index >= (uint)m_length)
			{
				ThrowArgumentOutOfRangeException(index);
			}
			int num = 1 << index;
			ref int reference = ref m_array[index >> 5];
			if (value)
			{
				reference |= num;
			}
			else
			{
				reference &= ~num;
			}
			_version++;
		}

		public void SetAll(bool value)
		{
			int num = (value ? (-1) : 0);
			int[] array = m_array;
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = num;
			}
			_version++;
		}

		public BitArray Not()
		{
			int[] array = m_array;
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = ~array[i];
			}
			_version++;
			return this;
		}

		public BitArray RightShift(int count)
		{
			if (count <= 0)
			{
				if (count < 0)
				{
					throw new ArgumentOutOfRangeException();
				}
				_version++;
				return this;
			}
			int num = 0;
			int int32ArrayLengthFromBitLength = GetInt32ArrayLengthFromBitLength(m_length);
			if (count < m_length)
			{
				int remainder;
				int num2 = Div32Rem(count, out remainder);
				Div32Rem(m_length, out var remainder2);
				if (remainder == 0)
				{
					uint num3 = uint.MaxValue >> 32 - remainder2;
					m_array[int32ArrayLengthFromBitLength - 1] &= (int)num3;
					Array.Copy(m_array, num2, m_array, 0, int32ArrayLengthFromBitLength - num2);
					num = int32ArrayLengthFromBitLength - num2;
				}
				else
				{
					int num4 = int32ArrayLengthFromBitLength - 1;
					while (num2 < num4)
					{
						uint num5 = (uint)m_array[num2] >> remainder;
						int num6 = m_array[++num2] << 32 - remainder;
						m_array[num++] = num6 | (int)num5;
					}
					uint num7 = uint.MaxValue >> 32 - remainder2;
					num7 &= (uint)m_array[num2];
					m_array[num++] = (int)(num7 >> remainder);
				}
			}
			m_array.AsSpan(num, int32ArrayLengthFromBitLength - num).Clear();
			_version++;
			return this;
		}

		public BitArray LeftShift(int count)
		{
			if (count <= 0)
			{
				if (count < 0)
				{
					throw new ArgumentOutOfRangeException();
				}
				_version++;
				return this;
			}
			int num2;
			if (count < m_length)
			{
				int num = m_length - 1 >> 5;
				num2 = Div32Rem(count, out var remainder);
				if (remainder == 0)
				{
					Array.Copy(m_array, 0, m_array, num2, num + 1 - num2);
				}
				else
				{
					int num3 = num - num2;
					while (num3 > 0)
					{
						int num4 = m_array[num3] << remainder;
						uint num5 = (uint)m_array[--num3] >> 32 - remainder;
						m_array[num] = num4 | (int)num5;
						num--;
					}
					m_array[num] = m_array[num3] << remainder;
				}
			}
			else
			{
				num2 = GetInt32ArrayLengthFromBitLength(m_length);
			}
			m_array.AsSpan(0, num2).Clear();
			_version++;
			return this;
		}

		public void CopyTo(Array array, int index)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			if (array.Rank != 1)
			{
				throw new ArgumentException();
			}
			int[] array2 = array as int[];
			if (array2 != null)
			{
				Div32Rem(m_length, out var remainder);
				if (remainder == 0)
				{
					Array.Copy(m_array, 0, array2, index, m_array.Length);
					return;
				}
				int num = m_length - 1 >> 5;
				Array.Copy(m_array, 0, array2, index, num);
				array2[index + num] = m_array[num] & ((1 << remainder) - 1);
				return;
			}
			byte[] array3 = array as byte[];
			if (array3 != null)
			{
				int num2 = GetByteArrayLengthFromBitLength(m_length);
				if (array.Length - index < num2)
				{
					throw new ArgumentException();
				}
				uint num3 = (uint)m_length & 7u;
				if (num3 != 0)
				{
					num2--;
				}
				Span<byte> destination = array3.AsSpan(index);
				int remainder2;
				int num4 = Div4Rem(num2, out remainder2);
				for (int i = 0; i < num4; i++)
				{
					BinaryPrimitives.WriteInt32LittleEndian(destination, m_array[i]);
					destination = destination.Slice(4);
				}
				if (num3 != 0)
				{
					destination[remainder2] = (byte)((m_array[num4] >> remainder2 * 8) & ((1 << (int)num3) - 1));
				}
				switch (remainder2)
				{
					default:
						return;
					case 3:
						destination[2] = (byte)(m_array[num4] >> 16);
						goto case 2;
					case 2:
						destination[1] = (byte)(m_array[num4] >> 8);
						break;
					case 1:
						break;
				}
				destination[0] = (byte)m_array[num4];
			}
			else
			{
				bool[] array4 = array as bool[];
				if (array4 == null)
				{
					throw new ArgumentException();
				}
				if (array.Length - index < m_length)
				{
					throw new ArgumentException();
				}
				for (int j = 0; j < m_length; j++)
				{
					int remainder3;
					int num5 = Div32Rem(j, out remainder3);
					array4[index + j] = ((m_array[num5] >> remainder3) & 1) != 0;
				}
			}
		}

		public object Clone()
		{
			return new BitArray(this);
		}

		public IEnumerator GetEnumerator()
		{
			return new BitArrayEnumeratorSimple(this);
		}

		private static int GetInt32ArrayLengthFromBitLength(int n)
		{
			return (int)((uint)(n - 1 + 32) >> 5);
		}

		private static int GetInt32ArrayLengthFromByteLength(int n)
		{
			return (int)((uint)(n - 1 + 4) >> 2);
		}

		private static int GetByteArrayLengthFromBitLength(int n)
		{
			return (int)((uint)(n - 1 + 8) >> 3);
		}

		private static int Div32Rem(int number, out int remainder)
		{
			uint result = (uint)number / 32u;
			remainder = number & 0x1F;
			return (int)result;
		}

		private static int Div4Rem(int number, out int remainder)
		{
			uint result = (uint)number / 4u;
			remainder = number & 3;
			return (int)result;
		}

		private static void ThrowArgumentOutOfRangeException(int index)
		{
			throw new ArgumentOutOfRangeException();
		}
	}
}
