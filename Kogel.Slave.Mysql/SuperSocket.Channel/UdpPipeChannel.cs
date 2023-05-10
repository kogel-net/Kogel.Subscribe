using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{
	public class UdpPipeChannel<TPackageInfo> : VirtualChannel<TPackageInfo>, IChannelWithSessionIdentifier
	{
		private Socket _socket;

		private bool _enableSendingPipe;

		public string SessionIdentifier { get; }

		public UdpPipeChannel(Socket socket, IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options, IPEndPoint remoteEndPoint)
			: this(socket, pipelineFilter, options, remoteEndPoint, $"{remoteEndPoint.Address}:{remoteEndPoint.Port}")
		{
		}

		public UdpPipeChannel(Socket socket, IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options, IPEndPoint remoteEndPoint, string sessionIdentifier)
			: base(pipelineFilter, options)
		{
			_socket = socket;
			_enableSendingPipe = "true".Equals(options.Values?["enableSendingPipe"], StringComparison.OrdinalIgnoreCase);
			base.RemoteEndPoint = remoteEndPoint;
			SessionIdentifier = sessionIdentifier;
		}

		protected override void Close()
		{
			WriteEOFPackage();
		}

		protected override ValueTask<int> FillPipeWithDataAsync(Memory<byte> memory, CancellationToken cancellationToken)
		{
			throw new NotSupportedException();
		}

		protected override async ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
		{
			if (_enableSendingPipe || buffer.IsSingleSegment)
			{
				int num = 0;
				ReadOnlySequence<byte>.Enumerator enumerator = buffer.GetEnumerator();
				while (enumerator.MoveNext())
				{
					ReadOnlyMemory<byte> current = enumerator.Current;
					int num2 = num;
					num = num2 + await _socket.SendToAsync(GetArrayByMemory(current), SocketFlags.None, base.RemoteEndPoint);
				}
				return num;
			}
			ArrayPool<byte> pool = ArrayPool<byte>.Shared;
			byte[] destBuffer = pool.Rent((int)buffer.Length);
			try
			{
				MergeBuffer(ref buffer, destBuffer);
				return await _socket.SendToAsync(new ArraySegment<byte>(destBuffer, 0, (int)buffer.Length), SocketFlags.None, base.RemoteEndPoint);
			}
			finally
			{
				pool.Return(destBuffer);
			}
		}

		protected override Task ProcessSends()
		{
			if (_enableSendingPipe)
			{
				return base.ProcessSends();
			}
			return Task.CompletedTask;
		}

		private void MergeBuffer(ref ReadOnlySequence<byte> buffer, byte[] destBuffer)
		{
			Span<byte> destination = destBuffer;
			int num = 0;
			ReadOnlySequence<byte>.Enumerator enumerator = buffer.GetEnumerator();
			while (enumerator.MoveNext())
			{
				ReadOnlyMemory<byte> current = enumerator.Current;
				current.Span.CopyTo(destination);
				num += current.Length;
				destination = destination.Slice(current.Length);
			}
		}

		public override async ValueTask SendAsync(ReadOnlyMemory<byte> buffer)
		{
			if (_enableSendingPipe)
			{
				await base.SendAsync(buffer);
			}
			else
			{
				await SendOverIOAsync(new ReadOnlySequence<byte>(buffer), CancellationToken.None);
			}
		}

		public override async ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package)
		{
			if (_enableSendingPipe)
			{
				await base.SendAsync(packageEncoder, package);
				return;
			}
			try
			{
				await base.SendLock.WaitAsync();
				PipeWriter writer = base.Out.Writer;
				WritePackageWithEncoder(writer, packageEncoder, package);
				await writer.FlushAsync();
				await ProcessOutputRead(base.Out.Reader, null);
			}
			finally
			{
				base.SendLock.Release();
			}
		}

		public override async ValueTask SendAsync(Action<PipeWriter> write)
		{
			if (_enableSendingPipe)
			{
				await base.SendAsync(write);
				return;
			}
			throw new NotSupportedException("The method SendAsync(Action<PipeWriter> write) cannot be used when noSendingPipe is true.");
		}
	}
}