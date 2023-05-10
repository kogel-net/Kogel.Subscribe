using System;
using System.Buffers;
using System.Text;

namespace SuperSocket.ProtoBase
{
	public class LinePipelineFilter : TerminatorPipelineFilter<TextPackageInfo>
	{
		protected Encoding Encoding { get; private set; }

		public LinePipelineFilter()
			: this(Encoding.UTF8)
		{
		}

		public LinePipelineFilter(Encoding encoding)
			: base((ReadOnlyMemory<byte>)new byte[2] { 13, 10 })
		{
			Encoding = encoding;
		}

		protected override TextPackageInfo DecodePackage(ref ReadOnlySequence<byte> buffer)
		{
			return new TextPackageInfo
			{
				Text = buffer.GetString(Encoding)
			};
		}
	}
}