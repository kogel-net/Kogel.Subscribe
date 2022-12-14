using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Client
{

	public interface IEasyClient<TReceivePackage, TSendPackage> : IEasyClient<TReceivePackage> where TReceivePackage : class
	{
		ValueTask SendAsync(TSendPackage package);
	}
	public interface IEasyClient<TReceivePackage> where TReceivePackage : class
	{
		IPEndPoint LocalEndPoint { get; set; }

		SecurityOptions Security { get; set; }

		event EventHandler Closed;

		event PackageHandler<TReceivePackage> PackageHandler;

		ValueTask<bool> ConnectAsync(EndPoint remoteEndPoint, CancellationToken cancellationToken = default(CancellationToken));

		ValueTask<TReceivePackage> ReceiveAsync();

		void StartReceive();

		ValueTask SendAsync(ReadOnlyMemory<byte> data);

		ValueTask SendAsync<TSendPackage>(IPackageEncoder<TSendPackage> packageEncoder, TSendPackage package);

		ValueTask CloseAsync();
	}
}