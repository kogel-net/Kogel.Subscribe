using System;

namespace SuperSocket.Channel
{
	public class CloseEventArgs : EventArgs
	{
		public CloseReason Reason { get; private set; }

		public CloseEventArgs(CloseReason reason)
		{
			Reason = reason;
		}
	}
}