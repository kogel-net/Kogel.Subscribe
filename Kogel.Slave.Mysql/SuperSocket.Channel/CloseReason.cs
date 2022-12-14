namespace SuperSocket.Channel
{

	public enum CloseReason
	{
		Unknown,
		ServerShutdown,
		RemoteClosing,
		LocalClosing,
		ApplicationError,
		SocketError,
		TimeOut,
		ProtocolError,
		InternalError
	}
}