using System.Collections.Generic;
using System.IO.Pipelines;
using Microsoft.Extensions.Logging;

namespace SuperSocket.Channel
{

	public class ChannelOptions
	{
		public int MaxPackageLength { get; set; } = 1048576;


		public int ReceiveBufferSize { get; set; } = 4096;


		public int SendBufferSize { get; set; } = 4096;


		public bool ReadAsDemand { get; set; }

		public int ReceiveTimeout { get; set; }

		public int SendTimeout { get; set; }

		public ILogger Logger { get; set; }

		public Pipe In { get; set; }

		public Pipe Out { get; set; }

		public Dictionary<string, string> Values { get; set; }
	}
}