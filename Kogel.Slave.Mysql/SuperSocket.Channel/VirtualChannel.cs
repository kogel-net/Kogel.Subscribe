using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{
	public abstract class VirtualChannel<TPackageInfo> : PipeChannel<TPackageInfo>, IVirtualChannel, IChannel
	{
		public VirtualChannel(IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options)
			: base(pipelineFilter, options)
		{
		}

		protected override Task FillPipeAsync(PipeWriter writer)
		{
			return Task.CompletedTask;
		}

		public async ValueTask<FlushResult> WritePipeDataAsync(Memory<byte> memory, CancellationToken cancellationToken)
		{
			return await base.In.Writer.WriteAsync(memory, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
	}
}