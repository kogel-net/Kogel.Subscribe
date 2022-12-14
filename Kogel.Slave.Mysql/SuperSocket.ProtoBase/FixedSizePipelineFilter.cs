using System.Buffers;

namespace SuperSocket.ProtoBase
{

	public class FixedSizePipelineFilter<TPackageInfo> : PipelineFilterBase<TPackageInfo> where TPackageInfo : class
	{
		private int _size;

		protected FixedSizePipelineFilter(int size)
		{
			_size = size;
		}

		public override TPackageInfo Filter(ref SequenceReader<byte> reader)
		{
			if (reader.Length < _size)
			{
				return null;
			}
			ReadOnlySequence<byte> buffer = reader.Sequence.Slice(0, _size);
			try
			{
				return DecodePackage(ref buffer);
			}
			finally
			{
				reader.Advance(_size);
			}
		}
	}
}