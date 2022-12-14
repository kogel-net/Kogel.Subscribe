using System.Threading.Tasks;

namespace SuperSocket.Channel
{

	internal interface IObjectPipe<T>
	{
		int Write(T target);

		ValueTask<T> ReadAsync();
	}
}