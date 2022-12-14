using System;
using System.Buffers;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace SuperSocket.Channel
{

	internal class DefaultObjectPipe<T> : IObjectPipe<T>, IValueTaskSource<T>, IDisposable
	{
		private class BufferSegment
		{
			public T[] Array { get; private set; }

			public BufferSegment Next { get; set; }

			public int Offset { get; set; }

			public int End { get; set; } = -1;


			public bool IsAvailable => Array.Length > End + 1;

			public BufferSegment(T[] array)
			{
				Array = array;
			}

			public void Write(T value)
			{
				Array[++End] = value;
			}
		}

		private const int _segmentSize = 5;

		private BufferSegment _first;

		private BufferSegment _current;

		private object _syncRoot = new object();

		private static readonly ArrayPool<T> _pool = ArrayPool<T>.Shared;

		private ManualResetValueTaskSourceCore<T> _taskSourceCore;

		private bool _waiting;

		private bool _lastReadIsWait;

		private int _length;

		private bool disposedValue;

		public DefaultObjectPipe()
		{
			SetBufferSegment(CreateSegment());
			_taskSourceCore = default(ManualResetValueTaskSourceCore<T>);
		}

		private BufferSegment CreateSegment()
		{
			return new BufferSegment(_pool.Rent(5));
		}

		private void SetBufferSegment(BufferSegment segment)
		{
			if (_first == null)
			{
				_first = segment;
			}
			BufferSegment current = _current;
			if (current != null)
			{
				current.Next = segment;
			}
			_current = segment;
		}

		public int Write(T target)
		{
			lock (_syncRoot)
			{
				if (_waiting)
				{
					_waiting = false;
					_taskSourceCore.SetResult(target);
					return _length;
				}
				BufferSegment bufferSegment = _current;
				if (!bufferSegment.IsAvailable)
				{
					bufferSegment = CreateSegment();
					SetBufferSegment(bufferSegment);
				}
				bufferSegment.Write(target);
				_length++;
				return _length;
			}
		}

		private bool TryRead(out T value)
		{
			BufferSegment first = _first;
			if (first.Offset < first.End)
			{
				value = first.Array[first.Offset];
				first.Array[first.Offset] = default(T);
				first.Offset++;
				return true;
			}
			if (first.Offset == first.End)
			{
				if (first == _current)
				{
					value = first.Array[first.Offset];
					first.Array[first.Offset] = default(T);
					first.Offset = 0;
					first.End = -1;
					return true;
				}
				value = first.Array[first.Offset];
				first.Array[first.Offset] = default(T);
				_first = first.Next;
				_pool.Return(first.Array);
				return true;
			}
			value = default(T);
			return false;
		}

		public ValueTask<T> ReadAsync()
		{
			lock (_syncRoot)
			{
				if (TryRead(out var value))
				{
					if (_lastReadIsWait)
					{
						_taskSourceCore.Reset();
						_lastReadIsWait = false;
					}
					_length--;
					if (_length == 0)
					{
						OnWaitTaskStart();
					}
					return new ValueTask<T>(value);
				}
				_waiting = true;
				_lastReadIsWait = true;
				_taskSourceCore.Reset();
				OnWaitTaskStart();
				return new ValueTask<T>(this, _taskSourceCore.Version);
			}
		}

		public T Read()
        {
			lock (_syncRoot)
			{
				if (TryRead(out var value))
				{
					if (_lastReadIsWait)
					{
						_taskSourceCore.Reset();
						_lastReadIsWait = false;
					}
					_length--;
					if (_length == 0)
					{
						OnWaitTaskStart();
					}
					return value;
				}
				_waiting = true;
				_lastReadIsWait = true;
				_taskSourceCore.Reset();
				OnWaitTaskStart();
				return new ValueTask<T>(this, _taskSourceCore.Version).Result;
			}
		}

		protected virtual void OnWaitTaskStart()
		{
		}

		T IValueTaskSource<T>.GetResult(short token)
		{
			return _taskSourceCore.GetResult(token);
		}

		ValueTaskSourceStatus IValueTaskSource<T>.GetStatus(short token)
		{
			return _taskSourceCore.GetStatus(token);
		}

		void IValueTaskSource<T>.OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
		{
			_taskSourceCore.OnCompleted(continuation, state, token, flags);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposedValue)
			{
				return;
			}
			if (disposing)
			{
				lock (_syncRoot)
				{
					for (BufferSegment bufferSegment = _first; bufferSegment != null; bufferSegment = bufferSegment.Next)
					{
						_pool.Return(bufferSegment.Array);
					}
					_first = null;
					_current = null;
				}
			}
			disposedValue = true;
		}

		void IDisposable.Dispose()
		{
			Dispose(disposing: true);
		}
	}
}