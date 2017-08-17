
namespace BeatThat
{
	/// <summary>
	/// Should be implemented by any type of web request to provide HTTP response code access
	/// </summary>
	public interface HasResponseCode  
	{
		long GetResponseCode();
	}

	public static class HasResponseCodeExt
	{
		/// <summary>
		/// Returns one of 3 common response codes for a request:
		/// 200 - success
		/// 404 - item request and item is null
		/// 500 - any other error
		/// </summary>
		/// <returns>The response code.</returns>
		/// <param name="r">The red component.</param>
		public static long DetermineResponseCode(this Request r)
		{
			var hasRC = r as HasResponseCode;

			if(hasRC != null) {
				var rc = hasRC.GetResponseCode();
				if(rc != 0) {
					return rc;
				}
			}

			if(r.hasError) {
				return 500;
			}

			var hasItem = r as ItemRequest;
			if(hasItem != null && hasItem.GetItem() == null) {
				return 404;
			}

			return 200;
		}
	}
}