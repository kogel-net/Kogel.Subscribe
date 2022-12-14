using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace SuperSocket.Channel
{

	internal class DefaultObjectPipeWithSupplyControl<T> : DefaultObjectPipe<T>, IValueTaskSource, ISupplyController
	{
		private ManualResetValueTaskSourceCore<bool> _taskSourceCore;

		private short _currentTaskVersion;

		public DefaultObjectPipeWithSupplyControl()
		{
			_taskSourceCore = new ManualResetValueTaskSourceCore<bool>
			{
				RunContinuationsAsynchronously = true
			};
		}

		public ValueTask SupplyRequired()
		{
			lock (this)
			{
				if (_currentTaskVersion == -1)
				{
					_currentTaskVersion = 0;
					return default(ValueTask);
				}
				_taskSourceCore.Reset();
				_currentTaskVersion = _taskSourceCore.Version;
				return new ValueTask(this, _taskSourceCore.Version);
			}
		}

		protected override void OnWaitTaskStart()
		{
			SetTaskCompleted(result: true);
		}

		public void SupplyEnd()
		{
			SetTaskCompleted(result: false);
		}

		private void SetTaskCompleted(bool result)
		{
			lock (this)
			{
				if (_currentTaskVersion == 0)
				{
					_currentTaskVersion = -1;
					return;
				}
				_taskSourceCore.SetResult(result);
				_currentTaskVersion = 0;
			}
		}

		void IValueTaskSource.GetResult(short token)
		{
			_taskSourceCore.GetResult(token);
		}

		ValueTaskSourceStatus IValueTaskSource.GetStatus(short token)
		{
			return _taskSourceCore.GetStatus(token);
		}

		void IValueTaskSource.OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
		{
			_taskSourceCore.OnCompleted(continuation, state, token, flags);
		}
	}
}