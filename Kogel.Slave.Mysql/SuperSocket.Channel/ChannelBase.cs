using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{
	public abstract class ChannelBase<TPackageInfo> : IChannel<TPackageInfo>, IChannel
	{
		public bool IsClosed { get; private set; }

		public EndPoint RemoteEndPoint { get; protected set; }

		public EndPoint LocalEndPoint { get; protected set; }

		public CloseReason? CloseReason { get; protected set; }

		public DateTimeOffset LastActiveTime { get; protected set; } = DateTimeOffset.Now;


		public event EventHandler<CloseEventArgs> Closed;

		public abstract void Start();

		public abstract IAsyncEnumerable<TPackageInfo> RunAsync();

		public abstract ValueTask SendAsync(ReadOnlyMemory<byte> buffer);

		public abstract ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package);

		public abstract ValueTask SendAsync(Action<PipeWriter> write);

		protected virtual void OnClosed()
		{
			IsClosed = true;
			EventHandler<CloseEventArgs> closed = this.Closed;
			if (closed != null && !(Interlocked.CompareExchange(ref this.Closed, null, closed) != closed))
			{
				CloseReason reason = (CloseReason.HasValue ? CloseReason.Value : SuperSocket.Channel.CloseReason.Unknown);
				closed(this, new CloseEventArgs(reason));
			}
		}

		public abstract ValueTask CloseAsync(CloseReason closeReason);

		public abstract ValueTask DetachAsync();
	}
}