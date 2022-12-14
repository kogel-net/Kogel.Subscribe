using System;
using System.Buffers;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{

	public class StreamPipeChannel<TPackageInfo> : PipeChannel<TPackageInfo>
	{
		private Stream _stream;

		public StreamPipeChannel(Stream stream, EndPoint remoteEndPoint, IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options)
			: this(stream, remoteEndPoint, (EndPoint)null, pipelineFilter, options)
		{
		}

		public StreamPipeChannel(Stream stream, EndPoint remoteEndPoint, EndPoint localEndPoint, IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options)
			: base(pipelineFilter, options)
		{
			_stream = stream;
			base.RemoteEndPoint = remoteEndPoint;
			base.LocalEndPoint = localEndPoint;
		}

		protected override void Close()
		{
			_stream.Close();
		}

		protected override void OnClosed()
		{
			_stream = null;
			base.OnClosed();
		}

		protected override async ValueTask<int> FillPipeWithDataAsync(Memory<byte> memory, CancellationToken cancellationToken)
		{
			return await _stream.ReadAsync(memory, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}

		protected override async ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
		{
			int total = 0;
			ReadOnlySequence<byte>.Enumerator enumerator = buffer.GetEnumerator();
			while (enumerator.MoveNext())
			{
				ReadOnlyMemory<byte> data = enumerator.Current;
				await _stream.WriteAsync(data, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				total += data.Length;
			}
			await _stream.FlushAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			return total;
		}

		protected override bool IsIgnorableException(Exception e)
		{
			if (base.IsIgnorableException(e))
			{
				return true;
			}
			if (e is SocketException se && se.IsIgnorableSocketException())
			{
				return true;
			}
			return false;
		}
	}
}