using System;
using System.IO;
using System.Net.Sockets;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;

namespace SuperSocket.Client
{
	public class ConnectState
	{
		public static readonly ConnectState CancelledState = new ConnectState(cancelled: false);

		public bool Result { get; set; }

		public bool Cancelled { get; private set; }

		public Exception Exception { get; set; }

		public Socket Socket { get; set; }

		public Stream Stream { get; set; }

		public ConnectState()
		{
		}

		private ConnectState(bool cancelled)
		{
			Cancelled = cancelled;
		}

		public IChannel<TReceivePackage> CreateChannel<TReceivePackage>(IPipelineFilter<TReceivePackage> pipelineFilter, ChannelOptions channelOptions) where TReceivePackage : class
		{
			Stream stream = Stream;
			Socket socket = Socket;
			if (stream != null)
			{
				return new StreamPipeChannel<TReceivePackage>(stream, socket.RemoteEndPoint, socket.LocalEndPoint, pipelineFilter, channelOptions);
			}
			return new TcpPipeChannel<TReceivePackage>(socket, pipelineFilter, channelOptions);
		}
	}
}