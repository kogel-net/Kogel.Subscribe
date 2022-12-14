using System;
using System.Buffers;

namespace SuperSocket.ProtoBase
{

	public class SequenceSegment : ReadOnlySequenceSegment<byte>, IDisposable
	{
		private bool disposedValue;

		private byte[] _pooledBuffer;

		private bool _pooled;

		public SequenceSegment(byte[] pooledBuffer, int length, bool pooled)
		{
			_pooledBuffer = pooledBuffer;
			_pooled = pooled;
			base.Memory = new ArraySegment<byte>(pooledBuffer, 0, length);
		}

		public SequenceSegment(byte[] pooledBuffer, int length)
			: this(pooledBuffer, length, pooled: true)
		{
		}

		public SequenceSegment(ReadOnlyMemory<byte> memory)
		{
			base.Memory = memory;
		}

		public SequenceSegment SetNext(SequenceSegment segment)
		{
			segment.RunningIndex = base.RunningIndex + base.Memory.Length;
			base.Next = segment;
			return segment;
		}

		public static SequenceSegment CopyFrom(ReadOnlyMemory<byte> memory)
		{
			byte[] array = ArrayPool<byte>.Shared.Rent(memory.Length);
			memory.Span.CopyTo(new Span<byte>(array));
			return new SequenceSegment(array, memory.Length);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing && _pooled && _pooledBuffer != null)
				{
					ArrayPool<byte>.Shared.Return(_pooledBuffer);
				}
				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}