using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{
	public class TcpPipeChannel<TPackageInfo> : PipeChannel<TPackageInfo>
	{
		private Socket _socket;

		private List<ArraySegment<byte>> _segmentsForSend;

		public TcpPipeChannel(Socket socket, IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options)
			: base(pipelineFilter, options)
		{
			_socket = socket;
			base.RemoteEndPoint = socket.RemoteEndPoint;
			base.LocalEndPoint = socket.LocalEndPoint;
		}

		protected override void OnClosed()
		{
			_socket = null;
			base.OnClosed();
		}

		protected override async ValueTask<int> FillPipeWithDataAsync(Memory<byte> memory, CancellationToken cancellationToken)
		{
			return await ReceiveAsync(_socket, memory, SocketFlags.None, cancellationToken);
		}

		private async ValueTask<int> ReceiveAsync(Socket socket, Memory<byte> memory, SocketFlags socketFlags, CancellationToken cancellationToken)
		{
			return await socket.ReceiveAsync(GetArrayByMemory<byte>(memory), socketFlags, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}

		protected override async ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
		{
			if (buffer.IsSingleSegment)
			{
				return await _socket.SendAsync(GetArrayByMemory(buffer.First), SocketFlags.None, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			if (_segmentsForSend == null)
			{
				_segmentsForSend = new List<ArraySegment<byte>>();
			}
			else
			{
				_segmentsForSend.Clear();
			}
			_ = _segmentsForSend;
			ReadOnlySequence<byte>.Enumerator enumerator = buffer.GetEnumerator();
			while (enumerator.MoveNext())
			{
				ReadOnlyMemory<byte> current = enumerator.Current;
				cancellationToken.ThrowIfCancellationRequested();
				_segmentsForSend.Add(GetArrayByMemory(current));
			}
			cancellationToken.ThrowIfCancellationRequested();
			return await _socket.SendAsync(_segmentsForSend, SocketFlags.None).ConfigureAwait(continueOnCapturedContext: false);
		}

		protected override void Close()
		{
			Socket socket = _socket;
			if (socket == null || Interlocked.CompareExchange(ref _socket, null, socket) != socket)
			{
				return;
			}
			try
			{
				socket.Shutdown(SocketShutdown.Both);
			}
			finally
			{
				socket.Close();
			}
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
