using System;
using System.Buffers;

namespace SuperSocket.ProtoBase
{

	public class TerminatorPipelineFilter<TPackageInfo> : PipelineFilterBase<TPackageInfo> where TPackageInfo : class
	{
		private readonly ReadOnlyMemory<byte> _terminator;

		public TerminatorPipelineFilter(ReadOnlyMemory<byte> terminator)
		{
			_terminator = terminator;
		}

		public override TPackageInfo Filter(ref SequenceReader<byte> reader)
		{
			ReadOnlyMemory<byte> terminator = _terminator;
			ReadOnlySpan<byte> span = terminator.Span;
			if (!reader.TryReadTo(out var sequence, span, advancePastDelimiter: false))
			{
				return null;
			}
			try
			{
				return DecodePackage(ref sequence);
			}
			finally
			{
				reader.Advance(terminator.Length);
			}
		}
	}
}