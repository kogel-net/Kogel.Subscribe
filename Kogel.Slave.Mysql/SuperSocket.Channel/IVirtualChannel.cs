using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Channel
{
	public interface IVirtualChannel : IChannel
	{
		ValueTask<FlushResult> WritePipeDataAsync(Memory<byte> memory, CancellationToken cancellationToken);
	}
}