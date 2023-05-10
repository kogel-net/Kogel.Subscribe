using System;

namespace SuperSocket.ProtoBase
{
	public class CommandLinePipelineFilter : TerminatorPipelineFilter<StringPackageInfo>
	{
		public CommandLinePipelineFilter()
			: base((ReadOnlyMemory<byte>)new byte[2] { 13, 10 })
		{
		}
	}
}