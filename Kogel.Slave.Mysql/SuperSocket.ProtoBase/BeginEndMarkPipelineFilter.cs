using System;
using System.Buffers;

namespace SuperSocket.ProtoBase
{
	public abstract class BeginEndMarkPipelineFilter<TPackageInfo> : PipelineFilterBase<TPackageInfo> where TPackageInfo : class
	{
		private readonly ReadOnlyMemory<byte> _beginMark;

		private readonly ReadOnlyMemory<byte> _endMark;

		private bool _foundBeginMark;

		protected BeginEndMarkPipelineFilter(ReadOnlyMemory<byte> beginMark, ReadOnlyMemory<byte> endMark)
		{
			_beginMark = beginMark;
			_endMark = endMark;
		}

		public override TPackageInfo Filter(ref SequenceReader<byte> reader)
		{
			if (!_foundBeginMark)
			{
				ReadOnlySpan<byte> span = _beginMark.Span;
				if (!reader.IsNext(span, advancePast: true))
				{
					throw new ProtocolException("Invalid beginning part of the package.");
				}
				_foundBeginMark = true;
			}
			ReadOnlySpan<byte> span2 = _endMark.Span;
			if (!reader.TryReadTo(out var sequence, span2, advancePastDelimiter: false))
			{
				return null;
			}
			reader.Advance(span2.Length);
			return DecodePackage(ref sequence);
		}

		public override void Reset()
		{
			base.Reset();
			_foundBeginMark = false;
		}
	}
}