using System.Net.Sockets;

namespace SuperSocket.Channel
{
	public static class SocketExtensions
	{
		public static bool IsIgnorableSocketException(this SocketException se)
		{
			switch (se.SocketErrorCode)
			{
				case SocketError.OperationAborted:
				case SocketError.NetworkReset:
				case SocketError.ConnectionReset:
				case SocketError.TimedOut:
					return true;
				default:
					return false;
			}
		}
	}
}