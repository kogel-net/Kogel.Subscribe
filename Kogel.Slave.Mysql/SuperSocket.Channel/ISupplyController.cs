using System.Threading.Tasks;

namespace SuperSocket.Channel
{
	internal interface ISupplyController
	{
		ValueTask SupplyRequired();

		void SupplyEnd();
	}
}