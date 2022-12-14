using Kogel.Slave.Mysql.Extension;
using System.Buffers;

namespace SuperSocket.ProtoBase
{

	public abstract class FixedHeaderPipelineFilter<TPackageInfo> : FixedSizePipelineFilter<TPackageInfo> where TPackageInfo : class
	{
		private bool _foundHeader;

		private readonly int _headerSize;

		private int _totalSize;

		protected FixedHeaderPipelineFilter(int headerSize)
			: base(headerSize)
		{
			_headerSize = headerSize;
		}

		public override TPackageInfo Filter(ref SequenceReader<byte> reader)
		{
			if (!_foundHeader)
			{
				if (reader.Length < _headerSize)
				{
					return null;
				}
				ReadOnlySequence<byte> buffer = reader.Sequence.Slice(0, _headerSize);
				int bodyLengthFromHeader = GetBodyLengthFromHeader(ref buffer);
				if (bodyLengthFromHeader < 0)
				{
					throw new ProtocolException("Failed to get body length from the package header.");
				}
				if (bodyLengthFromHeader == 0)
				{
					try
					{
						return DecodePackage(ref buffer);
					}
					finally
					{
						reader.Advance(_headerSize);
					}
				}
				_foundHeader = true;
				_totalSize = _headerSize + bodyLengthFromHeader;
			}
			int totalSize = _totalSize;
			if (reader.Length < totalSize)
			{
				return null;
			}
			ReadOnlySequence<byte> buffer2 = reader.Sequence.Slice(0, totalSize);
			try
			{
				return DecodePackage(ref buffer2);
			}
			finally
			{
				reader.Advance(totalSize);
			}
		}

		protected abstract int GetBodyLengthFromHeader(ref ReadOnlySequence<byte> buffer);

		public override void Reset()
		{
			base.Reset();
			_foundHeader = false;
			_totalSize = 0;
		}
	}
}