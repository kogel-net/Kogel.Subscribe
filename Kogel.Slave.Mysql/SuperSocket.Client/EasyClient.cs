using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;

namespace SuperSocket.Client
{
	public class EasyClient<TPackage, TSendPackage> : EasyClient<TPackage>, IEasyClient<TPackage, TSendPackage>, IEasyClient<TPackage> where TPackage : class
	{
		private IPackageEncoder<TSendPackage> _packageEncoder;

		protected EasyClient(IPackageEncoder<TSendPackage> packageEncoder)
		{
			_packageEncoder = packageEncoder;
		}

		public EasyClient(IPipelineFilter<TPackage> pipelineFilter, IPackageEncoder<TSendPackage> packageEncoder, ILogger logger = null)
			: this(pipelineFilter, packageEncoder, new ChannelOptions
			{
				Logger = logger
			})
		{
		}

		public EasyClient(IPipelineFilter<TPackage> pipelineFilter, IPackageEncoder<TSendPackage> packageEncoder, ChannelOptions options)
			: base(pipelineFilter, options)
		{
			_packageEncoder = packageEncoder;
		}

		public virtual async ValueTask SendAsync(TSendPackage package)
		{
			await SendAsync(_packageEncoder, package);
		}

		public new IEasyClient<TPackage, TSendPackage> AsClient()
		{
			return this;
		}
	}
	public class EasyClient<TReceivePackage> : IEasyClient<TReceivePackage> where TReceivePackage : class
	{
		private IPipelineFilter<TReceivePackage> _pipelineFilter;

		private IAsyncEnumerator<TReceivePackage> _packageStream;

		protected IChannel<TReceivePackage> Channel { get; private set; }

		protected ILogger Logger { get; set; }

		protected ChannelOptions Options { get; private set; }

		public IPEndPoint LocalEndPoint { get; set; }

		public SecurityOptions Security { get; set; }

		public event PackageHandler<TReceivePackage> PackageHandler;

		public event EventHandler Closed;

		protected EasyClient()
		{
		}

		public EasyClient(IPipelineFilter<TReceivePackage> pipelineFilter)
			: this(pipelineFilter, (ILogger)NullLogger.Instance)
		{
		}

		public EasyClient(IPipelineFilter<TReceivePackage> pipelineFilter, ILogger logger)
			: this(pipelineFilter, new ChannelOptions
			{
				Logger = logger
			})
		{
		}

		public EasyClient(IPipelineFilter<TReceivePackage> pipelineFilter, ChannelOptions options)
		{
			if (pipelineFilter == null)
			{
				throw new ArgumentNullException("pipelineFilter");
			}
			if (options == null)
			{
				throw new ArgumentNullException("options");
			}
			_pipelineFilter = pipelineFilter;
			Options = options;
			Logger = options.Logger;
		}

		public virtual IEasyClient<TReceivePackage> AsClient()
		{
			return this;
		}

		protected virtual IConnector GetConnector()
		{
			SecurityOptions security = Security;
			if (security != null && security.EnabledSslProtocols != 0)
			{
				return new SocketConnector(LocalEndPoint, new SslStreamConnector(security));
			}
			return new SocketConnector(LocalEndPoint);
		}

		ValueTask<bool> IEasyClient<TReceivePackage>.ConnectAsync(EndPoint remoteEndPoint, CancellationToken cancellationToken)
		{
			return ConnectAsync(remoteEndPoint, cancellationToken);
		}

		protected virtual async ValueTask<bool> ConnectAsync(EndPoint remoteEndPoint, CancellationToken cancellationToken)
		{
			ConnectState connectState = await GetConnector().ConnectAsync(remoteEndPoint, null, cancellationToken);
			if (connectState.Cancelled || cancellationToken.IsCancellationRequested)
			{
				OnError($"The connection to {remoteEndPoint} was cancelled.", connectState.Exception);
				return false;
			}
			if (!connectState.Result)
			{
				OnError($"Failed to connect to {remoteEndPoint}", connectState.Exception);
				return false;
			}
			if (connectState.Socket == null)
			{
				throw new Exception("Socket is null.");
			}
			ChannelOptions options = Options;
			SetupChannel(connectState.CreateChannel(_pipelineFilter, options));
			return true;
		}

		public void AsUdp(IPEndPoint remoteEndPoint, ArrayPool<byte> bufferPool = null, int bufferSize = 4096)
		{
			IPEndPoint iPEndPoint = LocalEndPoint;
			if (iPEndPoint == null)
			{
				iPEndPoint = new IPEndPoint((remoteEndPoint.AddressFamily == AddressFamily.InterNetworkV6) ? IPAddress.IPv6Any : IPAddress.Any, 0);
			}
			Socket socket = new Socket(remoteEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
			socket.Bind(iPEndPoint);
			UdpPipeChannel<TReceivePackage> channel = new UdpPipeChannel<TReceivePackage>(socket, _pipelineFilter, Options, remoteEndPoint);
			SetupChannel(channel);
			UdpReceive(socket, channel, bufferPool, bufferSize);
		}

		private async void UdpReceive(Socket socket, UdpPipeChannel<TReceivePackage> channel, ArrayPool<byte> bufferPool, int bufferSize)
		{
			if (bufferPool == null)
			{
				bufferPool = ArrayPool<byte>.Shared;
			}
			while (true)
			{
				byte[] buffer = bufferPool.Rent(bufferSize);
				try
				{
					await channel.WritePipeDataAsync(new ArraySegment<byte>(buffer, 0, (await socket.ReceiveFromAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), SocketFlags.None, channel.RemoteEndPoint).ConfigureAwait(continueOnCapturedContext: false)).ReceivedBytes).AsMemory(), CancellationToken.None);
				}
				catch (NullReferenceException)
				{
					break;
				}
				catch (ObjectDisposedException)
				{
					break;
				}
				catch (Exception exception)
				{
					OnError("Failed to receive UDP data.", exception);
				}
				finally
				{
					bufferPool.Return(buffer);
				}
			}
		}

		protected virtual void SetupChannel(IChannel<TReceivePackage> channel)
		{
			channel.Closed += OnChannelClosed;
			channel.Start();
			_packageStream = channel.GetPackageStream();
			Channel = channel;
		}

		ValueTask<TReceivePackage> IEasyClient<TReceivePackage>.ReceiveAsync()
		{
			return ReceiveAsync();
		}

		protected virtual async ValueTask<TReceivePackage> ReceiveAsync()
		{
			TReceivePackage val = await _packageStream.ReceiveAsync();
			if (val != null)
			{
				return val;
			}
			OnClosed(Channel, EventArgs.Empty);
			return null;
		}

		void IEasyClient<TReceivePackage>.StartReceive()
		{
			StartReceive();
		}

		protected virtual void StartReceive()
		{
			StartReceiveAsync();
		}

		private async void StartReceiveAsync()
		{
			IAsyncEnumerator<TReceivePackage> enumerator = _packageStream;
			while (await enumerator.MoveNextAsync())
			{
				await OnPackageReceived(enumerator.Current);
			}
		}

		protected virtual async ValueTask OnPackageReceived(TReceivePackage package)
		{
			PackageHandler<TReceivePackage> packageHandler = this.PackageHandler;
			try
			{
				await packageHandler(this, package);
			}
			catch (Exception exception)
			{
				OnError("Unhandled exception happened in PackageHandler.", exception);
			}
		}

		private void OnChannelClosed(object sender, EventArgs e)
		{
			Channel.Closed -= OnChannelClosed;
			OnClosed(this, e);
		}

		private void OnClosed(object sender, EventArgs e)
		{
			EventHandler closed = this.Closed;
			if (closed != null && Interlocked.CompareExchange(ref this.Closed, null, closed) == closed)
			{
				closed(sender, e);
			}
		}

		protected virtual void OnError(string message, Exception exception)
		{
			Logger?.LogError(exception, message);
		}

		protected virtual void OnError(string message)
		{
			Logger?.LogError(message);
		}

		ValueTask IEasyClient<TReceivePackage>.SendAsync(ReadOnlyMemory<byte> data)
		{
			return SendAsync(data);
		}

		protected virtual async ValueTask SendAsync(ReadOnlyMemory<byte> data)
		{
			await Channel.SendAsync(data);
		}

		ValueTask IEasyClient<TReceivePackage>.SendAsync<TSendPackage>(IPackageEncoder<TSendPackage> packageEncoder, TSendPackage package)
		{
			return SendAsync(packageEncoder, package);
		}

		protected virtual async ValueTask SendAsync<TSendPackage>(IPackageEncoder<TSendPackage> packageEncoder, TSendPackage package)
		{
			await Channel.SendAsync(packageEncoder, package);
		}

		public virtual async ValueTask CloseAsync()
		{
			await Channel.CloseAsync(CloseReason.LocalClosing);
			OnClosed(this, EventArgs.Empty);
		}
	}
}