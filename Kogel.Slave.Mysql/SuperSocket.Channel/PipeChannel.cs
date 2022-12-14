using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{

	public abstract class PipeChannel<TPackageInfo> : ChannelBase<TPackageInfo>, IChannel<TPackageInfo>, IChannel, IPipeChannel
	{
		private IPipelineFilter<TPackageInfo> _pipelineFilter;

		private CancellationTokenSource _cts = new CancellationTokenSource();

		private IObjectPipe<TPackageInfo> _packagePipe;

		private Task _readsTask;

		private Task _sendsTask;

		private bool _isDetaching;

		protected SemaphoreSlim SendLock { get; } = new SemaphoreSlim(1, 1);


		protected Pipe Out { get; }

		Pipe IPipeChannel.Out => Out;

		protected Pipe In { get; }

		Pipe IPipeChannel.In => In;

		IPipelineFilter IPipeChannel.PipelineFilter => _pipelineFilter;

		protected ILogger Logger { get; }

		protected ChannelOptions Options { get; }

		protected PipeChannel(IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options)
		{
			_pipelineFilter = pipelineFilter;
			if (!options.ReadAsDemand)
			{
				_packagePipe = new DefaultObjectPipe<TPackageInfo>();
			}
			else
			{
				_packagePipe = new DefaultObjectPipeWithSupplyControl<TPackageInfo>();
			}
			Options = options;
			Logger = options.Logger;
			Out = options.Out ?? new Pipe();
			In = options.In ?? new Pipe();
		}

		public override void Start()
		{
			_readsTask = ProcessReads();
			_sendsTask = ProcessSends();
			WaitHandleClosing();
		}

		private async void WaitHandleClosing()
		{
			await HandleClosing();
		}

		public override async IAsyncEnumerable<TPackageInfo> RunAsync()
		{
			if (_readsTask == null || _sendsTask == null)
			{
				throw new Exception("The channel has not been started yet.");
			}
			while (true)
			{
				TPackageInfo val = await _packagePipe.ReadAsync().ConfigureAwait(continueOnCapturedContext: false);
				if (val == null)
				{
					break;
				}
				yield return val;
			}
			await HandleClosing();
		}

		private async ValueTask HandleClosing()
		{
			try
			{
				await Task.WhenAll(_readsTask, _sendsTask);
			}
			catch (OperationCanceledException)
			{
			}
			catch (Exception e)
			{
				OnError("Unhandled exception in the method PipeChannel.Run.", e);
			}
			finally
			{
				if (!_isDetaching && !base.IsClosed)
				{
					Close();
					OnClosed();
				}
			}
		}

		protected abstract void Close();

		public override async ValueTask CloseAsync(CloseReason closeReason)
		{
			base.CloseReason = closeReason;
			_cts.Cancel();
			await HandleClosing();
		}

		protected virtual async Task FillPipeAsync(PipeWriter writer)
		{
			ChannelOptions options = Options;
			CancellationTokenSource cts = _cts;
			ISupplyController supplyController = _packagePipe as ISupplyController;
			if (supplyController != null)
			{
				cts.Token.Register(delegate
				{
					supplyController.SupplyEnd();
				});
			}
			while (!cts.IsCancellationRequested)
			{
				try
				{
					if (supplyController == null)
					{
						goto IL_0131;
					}
					await supplyController.SupplyRequired();
					if (!cts.IsCancellationRequested)
					{
						goto IL_0131;
					}
					goto end_IL_00a6;
				IL_0131:
					int num = options.ReceiveBufferSize;
					_ = options.MaxPackageLength;
					if (num <= 0)
					{
						num = 4096;
					}
					Memory<byte> memory = writer.GetMemory(num);
					int num2 = await FillPipeWithDataAsync(memory, cts.Token);
					if (num2 == 0)
					{
						if (!base.CloseReason.HasValue)
						{
							base.CloseReason = SuperSocket.Channel.CloseReason.RemoteClosing;
						}
						break;
					}
					base.LastActiveTime = DateTimeOffset.Now;
					writer.Advance(num2);
					goto IL_0280;
				end_IL_00a6:;
				}
				catch (Exception e)
				{
					if (!IsIgnorableException(e))
					{
						OnError("Exception happened in ReceiveAsync", e);
						if (!base.CloseReason.HasValue)
						{
							base.CloseReason = (cts.IsCancellationRequested ? SuperSocket.Channel.CloseReason.LocalClosing : SuperSocket.Channel.CloseReason.SocketError);
						}
					}
					else if (!base.CloseReason.HasValue)
					{
						base.CloseReason = SuperSocket.Channel.CloseReason.RemoteClosing;
					}
				}
				break;
			IL_0280:
				if ((await writer.FlushAsync()).IsCompleted)
				{
					break;
				}
			}
			writer.Complete();
			Out.Writer.Complete();
			//await writer.CompleteAsync().ConfigureAwait(continueOnCapturedContext: false);
			//await Out.Writer.CompleteAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		protected virtual bool IsIgnorableException(Exception e)
		{
			if (e is ObjectDisposedException || e is NullReferenceException || e is OperationCanceledException)
			{
				return true;
			}
			if (e.InnerException != null)
			{
				return IsIgnorableException(e.InnerException);
			}
			return false;
		}

		protected abstract ValueTask<int> FillPipeWithDataAsync(Memory<byte> memory, CancellationToken cancellationToken);

		protected virtual async Task ProcessReads()
		{
			Pipe @in = In;
			Task task = FillPipeAsync(@in.Writer);
			Task task2 = ReadPipeAsync(@in.Reader);
			await Task.WhenAll(task2, task);
		}

		protected async ValueTask<bool> ProcessOutputRead(PipeReader reader, CancellationTokenSource cts)
		{
			ReadResult readResult = await reader.ReadAsync(CancellationToken.None);
			bool completed = readResult.IsCompleted;
			ReadOnlySequence<byte> buffer = readResult.Buffer;
			SequencePosition end = buffer.End;
			if (!buffer.IsEmpty)
			{
				try
				{
					await SendOverIOAsync(buffer, CancellationToken.None);
					base.LastActiveTime = DateTimeOffset.Now;
				}
				catch (Exception e)
				{
					cts?.Cancel(throwOnFirstException: false);
					if (!IsIgnorableException(e))
					{
						OnError("Exception happened in SendAsync", e);
					}
					return true;
				}
			}
			reader.AdvanceTo(end);
			return completed;
		}

		protected virtual async Task ProcessSends()
		{
			PipeReader output = Out.Reader;
			CancellationTokenSource cts = _cts;
			while (!(await ProcessOutputRead(output, cts)))
			{
			}
			output.Complete();
		}

		private void CheckChannelOpen()
		{
			if (base.IsClosed)
			{
				throw new Exception("Channel is closed now, send is not allowed.");
			}
		}

		protected abstract ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken);

		public override async ValueTask SendAsync(ReadOnlyMemory<byte> buffer)
		{
			_ = 1;
			try
			{
				await SendLock.WaitAsync();
				PipeWriter writer = Out.Writer;
				WriteBuffer(writer, buffer);
				await writer.FlushAsync();
			}
			finally
			{
				SendLock.Release();
			}
		}

		private void WriteBuffer(PipeWriter writer, ReadOnlyMemory<byte> buffer)
		{
			CheckChannelOpen();
			writer.Write(buffer.Span);
		}

		public override async ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package)
		{
			_ = 1;
			try
			{
				await SendLock.WaitAsync();
				PipeWriter writer = Out.Writer;
				WritePackageWithEncoder(writer, packageEncoder, package);
				await writer.FlushAsync();
			}
			finally
			{
				SendLock.Release();
			}
		}

		public override async ValueTask SendAsync(Action<PipeWriter> write)
		{
			_ = 1;
			try
			{
				await SendLock.WaitAsync();
				PipeWriter writer = Out.Writer;
				write(writer);
				await writer.FlushAsync();
			}
			finally
			{
				SendLock.Release();
			}
		}

		protected void WritePackageWithEncoder<TPackage>(IBufferWriter<byte> writer, IPackageEncoder<TPackage> packageEncoder, TPackage package)
		{
			CheckChannelOpen();
			packageEncoder.Encode(writer, package);
		}

		protected internal ArraySegment<T> GetArrayByMemory<T>(ReadOnlyMemory<T> memory)
		{
			if (!MemoryMarshal.TryGetArray(memory, out var segment))
			{
				throw new InvalidOperationException("Buffer backed by array was expected");
			}
			return segment;
		}

		protected async Task ReadPipeAsync(PipeReader reader)
		{
			CancellationTokenSource cts = _cts;
			while (!cts.IsCancellationRequested)
			{
				ReadResult readResult;
				try
				{
					readResult = await reader.ReadAsync(cts.Token);
				}
				catch (Exception e)
				{
					if (!IsIgnorableException(e))
					{
						OnError("Failed to read from the pipe", e);
					}
					break;
				}
				ReadOnlySequence<byte> buffer = readResult.Buffer;
				SequencePosition consumed = buffer.Start;
				SequencePosition examined = buffer.End;
				if (readResult.IsCanceled)
				{
					break;
				}
				bool isCompleted = readResult.IsCompleted;
				try
				{
					if ((buffer.Length > 0 && !ReaderBuffer(ref buffer, out consumed, out examined)) || isCompleted)
					{
						break;
					}
					continue;
				}
				catch (Exception e2)
				{
					OnError("Protocol error", e2);
					Close();
				}
				finally
				{
					reader.AdvanceTo(consumed, examined);
				}
				break;
			}
			reader.Complete();
			WriteEOFPackage();
		}

		protected void WriteEOFPackage()
		{
			_packagePipe.Write(default(TPackageInfo));
		}

		private bool ReaderBuffer(ref ReadOnlySequence<byte> buffer, out SequencePosition consumed, out SequencePosition examined)
		{
			consumed = buffer.Start;
			examined = buffer.End;
			long num = 0L;
			int maxPackageLength = Options.MaxPackageLength;
			SequenceReader<byte> reader = new SequenceReader<byte>(buffer);
			while (true)
			{
				IPipelineFilter<TPackageInfo> pipelineFilter = _pipelineFilter;
				bool flag = false;
				TPackageInfo val = pipelineFilter.Filter(ref reader);
				IPipelineFilter<TPackageInfo> nextFilter = pipelineFilter.NextFilter;
				if (nextFilter != null)
				{
					nextFilter.Context = pipelineFilter.Context;
					_pipelineFilter = nextFilter;
					flag = true;
				}
				long consumed2 = reader.Consumed;
				num += consumed2;
				long num2 = consumed2;
				if (num2 == 0L)
				{
					num2 = reader.Length;
				}
				if (maxPackageLength > 0 && num2 > maxPackageLength)
				{
					OnError($"Package cannot be larger than {maxPackageLength}.");
					Close();
					return false;
				}
				if (val == null)
				{
					if (!flag)
					{
						consumed = buffer.GetPosition(num);
						return true;
					}
					pipelineFilter.Reset();
				}
				else
				{
					pipelineFilter.Reset();
					_packagePipe.Write(val);
				}
				if (reader.End)
				{
					break;
				}
				if (consumed2 > 0)
				{
					reader = new SequenceReader<byte>(reader.Sequence.Slice(consumed2));
				}
			}
			examined = (consumed = buffer.End);
			return true;
		}

		public override async ValueTask DetachAsync()
		{
			_isDetaching = true;
			_cts.Cancel();
			await HandleClosing();
			_isDetaching = false;
		}

		protected void OnError(string message, Exception e = null)
		{
			if (e != null)
			{
				Logger?.LogError(e, message);
			}
			else
			{
				Logger?.LogError(message);
			}
		}
	}
}