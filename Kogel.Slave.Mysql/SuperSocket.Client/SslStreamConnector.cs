using Kogel.Slave.Mysql.Extension;
using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Client
{

	public class SslStreamConnector : ConnectorBase
	{
		public SslClientAuthenticationOptions Options { get; private set; }

		public SslStreamConnector(SslClientAuthenticationOptions options)
		{
			Options = options;
		}

		public SslStreamConnector(SslClientAuthenticationOptions options, IConnector nextConnector)
			: base(nextConnector)
		{
			Options = options;
		}

		protected override async ValueTask<ConnectState> ConnectAsync(EndPoint remoteEndPoint, ConnectState state, CancellationToken cancellationToken)
		{
			string text = Options.TargetHost;
			if (string.IsNullOrEmpty(text))
			{
				if (remoteEndPoint is DnsEndPoint dnsEndPoint)
				{
					text = dnsEndPoint.Host;
				}
				else if (remoteEndPoint is IPEndPoint iPEndPoint)
				{
					text = iPEndPoint.Address.ToString();
				}
				Options.TargetHost = text;
			}
			Socket socket = state.Socket;
			if (socket == null)
			{
				throw new Exception("Socket from previous connector is null.");
			}
			try
			{
				SslStream stream = new SslStream(new NetworkStream(socket, ownsSocket: true), leaveInnerStreamOpen: false);
				await stream.AuthenticateAsClientAsync(Options.TargetHost);
				if (cancellationToken.IsCancellationRequested)
				{
					return ConnectState.CancelledState;
				}
				state.Stream = stream;
				return state;
			}
			catch (Exception exception)
			{
				return new ConnectState
				{
					Result = false,
					Exception = exception
				};
			}
		}
	}
}
