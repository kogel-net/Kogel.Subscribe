using System;
using System.Collections.Generic;
using System.Text;

namespace Kogel.Slave.Mysql.Extension
{
	public readonly struct SslApplicationProtocol : IEquatable<SslApplicationProtocol>
	{
		private static readonly Encoding s_utf8 = Encoding.GetEncoding(Encoding.UTF8.CodePage, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);

		private static readonly byte[] s_http2Utf8 = new byte[2] { 104, 50 };

		private static readonly byte[] s_http11Utf8 = new byte[8] { 104, 116, 116, 112, 47, 49, 46, 49 };

		public static readonly SslApplicationProtocol Http2 = new SslApplicationProtocol(s_http2Utf8, copy: false);

		public static readonly SslApplicationProtocol Http11 = new SslApplicationProtocol(s_http11Utf8, copy: false);

		private readonly byte[] _readOnlyProtocol;

		public ReadOnlyMemory<byte> Protocol => _readOnlyProtocol;

		internal SslApplicationProtocol(byte[] protocol, bool copy)
		{
			if (protocol.Length == 0 || protocol.Length > 255)
			{
				throw new ArgumentException();
			}
			_readOnlyProtocol = (copy ? protocol.AsSpan().ToArray() : protocol);
		}

		public SslApplicationProtocol(byte[] protocol)
			: this(protocol ?? throw new ArgumentNullException("protocol"), copy: true)
		{
		}

		public SslApplicationProtocol(string protocol)
			: this(s_utf8.GetBytes(protocol ?? throw new ArgumentNullException("protocol")), copy: false)
		{
		}

		public bool Equals(SslApplicationProtocol other)
		{
			return ((ReadOnlySpan<byte>)_readOnlyProtocol).SequenceEqual((ReadOnlySpan<byte>)other._readOnlyProtocol);
		}

		public override bool Equals(object obj)
		{
			if (obj is SslApplicationProtocol other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			byte[] readOnlyProtocol = _readOnlyProtocol;
			if (readOnlyProtocol == null)
			{
				return 0;
			}
			int num = 0;
			for (int i = 0; i < readOnlyProtocol.Length; i++)
			{
				num = ((num << 5) + num) ^ readOnlyProtocol[i];
			}
			return num;
		}

		public override string ToString()
		{
			byte[] readOnlyProtocol = _readOnlyProtocol;
			try
			{
				return (readOnlyProtocol == null) ? string.Empty : ((readOnlyProtocol == s_http2Utf8) ? "h2" : ((readOnlyProtocol == s_http11Utf8) ? "http/1.1" : s_utf8.GetString(readOnlyProtocol)));
			}
			catch
			{
				char[] array = new char[readOnlyProtocol.Length * 5];
				int num = 0;
				for (int j = 0; j < array.Length; j += 5)
				{
					byte a = readOnlyProtocol[num++];
					array[j] = '0';
					array[j + 1] = 'x';
					array[j + 2] = GetHexValue(Math.DivRem(a, 16, out var result));
					array[j + 3] = GetHexValue(result);
					array[j + 4] = ' ';
				}
				return new string(array, 0, array.Length - 1);
			}		
		}

		static char GetHexValue(int i)
		{
			return (char)((i < 10) ? (i + 48) : (i - 10 + 97));
		}

		public static bool operator ==(SslApplicationProtocol left, SslApplicationProtocol right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(SslApplicationProtocol left, SslApplicationProtocol right)
		{
			return !(left == right);
		}
	}
}
