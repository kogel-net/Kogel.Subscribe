using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{

	public class RelayPipeChannel<TPackageInfo> : PipeChannel<TPackageInfo> where TPackageInfo : class
	{
		private static ChannelOptions RebuildOptionsWithPipes(ChannelOptions options, Pipe pipeIn, Pipe pipeOut)
		{
			options.In = pipeIn;
			options.Out = pipeOut;
			return options;
		}

		public RelayPipeChannel(IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options, Pipe pipeIn, Pipe pipeOut)
			: base(pipelineFilter, RebuildOptionsWithPipes(options, pipeIn, pipeOut))
		{
		}

		protected override void Close()
		{
			base.In.Writer.Complete();
			base.Out.Writer.Complete();
		}

		protected override async ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
		{
			PipeWriter writer = base.Out.Writer;
			int total = 0;
			ReadOnlySequence<byte>.Enumerator enumerator = buffer.GetEnumerator();
			while (enumerator.MoveNext())
			{
				ReadOnlyMemory<byte> data = enumerator.Current;
				FlushResult flushResult = await writer.WriteAsync(data, cancellationToken);
				if (flushResult.IsCompleted)
				{
					total += data.Length;
				}
				else if (flushResult.IsCanceled)
				{
					break;
				}
			}
			return total;
		}

		protected override int FillPipeWithDataAsync(Memory<byte> memory)
		{
			throw new NotSupportedException();
		}
	}
}