using Kogel.Slave.Mysql.Extension;
using System.Buffers;

namespace SuperSocket.ProtoBase
{

	public interface IPipelineFilter
	{
		object Context { get; set; }

		void Reset();
	}
	public interface IPipelineFilter<TPackageInfo> : IPipelineFilter
	{
		IPackageDecoder<TPackageInfo> Decoder { get; set; }

		IPipelineFilter<TPackageInfo> NextFilter { get; }

		TPackageInfo Filter(ref SequenceReader<byte> reader);
	}
}