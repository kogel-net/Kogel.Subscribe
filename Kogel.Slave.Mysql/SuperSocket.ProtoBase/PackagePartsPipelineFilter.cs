using System.Buffers;

namespace SuperSocket.ProtoBase
{

	public abstract class PackagePartsPipelineFilter<TPackageInfo> : IPipelineFilter<TPackageInfo>, IPipelineFilter where TPackageInfo : class
	{
		private IPackagePartReader<TPackageInfo> _currentPartReader;

		protected TPackageInfo CurrentPackage { get; private set; }

		public object Context { get; set; }

		public IPackageDecoder<TPackageInfo> Decoder { get; set; }

		public IPipelineFilter<TPackageInfo> NextFilter { get; protected set; }

		protected abstract TPackageInfo CreatePackage();

		protected abstract IPackagePartReader<TPackageInfo> GetFirstPartReader();

		public virtual TPackageInfo Filter(ref SequenceReader<byte> reader)
		{
			TPackageInfo val = CurrentPackage;
			IPackagePartReader<TPackageInfo> packagePartReader = _currentPartReader;
			if (val == null)
			{
				TPackageInfo val3 = (CurrentPackage = CreatePackage());
				val = val3;
				packagePartReader = (_currentPartReader = GetFirstPartReader());
			}
			bool needMoreData;
			do
			{
				if (packagePartReader.Process(val, Context, ref reader, out var nextPartReader, out needMoreData))
				{
					Reset();
					return val;
				}
				if (nextPartReader != null)
				{
					_currentPartReader = nextPartReader;
					OnPartReaderSwitched(packagePartReader, nextPartReader);
					packagePartReader = nextPartReader;
				}
			}
			while (!needMoreData && reader.Remaining > 0);
			return null;
		}

		protected virtual void OnPartReaderSwitched(IPackagePartReader<TPackageInfo> currentPartReader, IPackagePartReader<TPackageInfo> nextPartReader)
		{
		}

		public virtual void Reset()
		{
			CurrentPackage = null;
			_currentPartReader = null;
		}
	}
}