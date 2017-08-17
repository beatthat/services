
namespace BeatThat
{
	/// <summary>
	/// Should be implemented by any type of web request to provide HTTP response code access
	/// </summary>
	public interface HasResponseText  
	{
		string GetResponseText();
	}

	public static class HasResponseTextExt
	{
		/// <summary>
		/// Returns one of 3 common response codes for a request:
		/// 200 - success
		/// 404 - item request and item is null
		/// 500 - any other error
		/// </summary>
		/// <returns>The response code.</returns>
		/// <param name="r">The red component.</param>
		public static string GetResponseText(this Request r)
		{
			var hasRT = r as HasResponseText;

			return hasRT != null? hasRT.GetResponseText(): null;
		}
	}
}