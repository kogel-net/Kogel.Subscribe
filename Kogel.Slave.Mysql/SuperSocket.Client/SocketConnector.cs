using Kogel.Slave.Mysql.Extension;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Client
{

	public class SocketConnector : ConnectorBase
	{
		public IPEndPoint LocalEndPoint { get; private set; }

		public SocketConnector()
		{
		}

		public SocketConnector(IConnector nextConnector)
			: base(nextConnector)
		{
		}

		public SocketConnector(IPEndPoint localEndPoint)
		{
			LocalEndPoint = localEndPoint;
		}

		public SocketConnector(IPEndPoint localEndPoint, IConnector nextConnector)
			: base(nextConnector)
		{
			LocalEndPoint = localEndPoint;
		}

		protected override async ValueTask<ConnectState> ConnectAsync(EndPoint remoteEndPoint, ConnectState state, CancellationToken cancellationToken)
		{
			Socket socket = new Socket(remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			try
			{
				IPEndPoint localEndPoint = LocalEndPoint;
				if (localEndPoint != null)
				{
					socket.ExclusiveAddressUse = false;
					socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
					socket.Bind(localEndPoint);
				}
				await socket.ConnectAsync(remoteEndPoint);
			}
			catch (Exception exception)
			{
				return new ConnectState
				{
					Result = false,
					Exception = exception
				};
			}
			return new ConnectState
			{
				Result = true,
				Socket = socket
			};
		}
	}
}