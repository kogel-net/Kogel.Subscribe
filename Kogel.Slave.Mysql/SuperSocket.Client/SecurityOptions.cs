using Kogel.Slave.Mysql.Extension;
using System.Net;
using System.Net.Security;

namespace SuperSocket.Client
{

	public class SecurityOptions : SslClientAuthenticationOptions
	{
		public NetworkCredential Credential { get; set; }
	}
}