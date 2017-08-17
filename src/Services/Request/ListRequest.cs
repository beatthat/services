
namespace BeatThat
{
	public interface ListRequest<T> : Request
	{
		T[] items { get; }
	}
}