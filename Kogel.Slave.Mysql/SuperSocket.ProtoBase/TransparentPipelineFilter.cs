using System.Buffers;

namespace SuperSocket.ProtoBase
{
	public class TransparentPipelineFilter<TPackageInfo> : PipelineFilterBase<TPackageInfo> where TPackageInfo : class
	{
		public override TPackageInfo Filter(ref SequenceReader<byte> reader)
		{
			ReadOnlySequence<byte> buffer = reader.Sequence;
			long remaining = reader.Remaining;
			TPackageInfo result = DecodePackage(ref buffer);
			reader.Advance(remaining);
			return result;
		}
	}
}