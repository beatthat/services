
namespace BeatThat
{
	/// <summary>
	/// Should be implemented by any type of web request to provide HTTP response code access
	/// </summary>
	public interface NetworkRequest : Request
	{
		bool isNetworkError { get; }
	}


}