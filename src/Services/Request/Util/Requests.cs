using System;

namespace BeatThat
{


	// placeholder do-nothing IDisposible to use when transition debugging is off
	class DebugRequestsDisabled : IDisposable 
	{
		public static readonly IDisposable INSTANCE = new DebugRequestsDisabled();

		public void Dispose () {} 
	}

	/// <summary>
	/// Static blackboard for global requests settings (e.g debugging)
	/// </summary>
	public static class Requests 
	{
		#if REQUESTS_DEBUG_ALL
		static Requests()
		{
			DebugStart();
		}
		#endif

		/// <summary>
		/// Returns a disposable that does nothing for convenience in places where a debug block can be turned on or off
		/// </summary>
		public static IDisposable DebugDisabled()
		{
			return DebugRequestsDisabled.INSTANCE;
		}

		/// <summary>
		/// Call to begin a debugging block. 
		/// Requests created after this call should have their debug property enabled.
		/// 
		/// To end the debug block, either call Requests.DebugEnd 
		/// or you can put the whole debug section in a using block, e.g.
		/// 
		/// 	using(Requests.DebugStart()) {
		/// 		// create and execute a bunch of requests
		/// 	} // debug block ended by IDisposable
		/// </summary>
		public static IDisposable DebugStart()
		{
			Requests.debugPinCount++;
			return new DebugBlock();
		}

		/// <summary>
		/// Call to end a debugging block. 
		/// Requests created after this call should have their debug property enabled.
		/// NOTE: implemented as a pincount, so debugging really ends 
		/// when the number of DebugEnd calls matches the number of DebugStart
		/// </summary>
		public static void DebugEnd()
		{
			if(Requests.debugPinCount == 0) {
				return;
			}
			Requests.debugPinCount--;
		}

		/// <summary>
		/// True if a debug block is active. 
		/// When debug is TRUE, new Requests should set their debug property TRUE.
		/// This behaviour is implemented in the constructor for RequestBase
		/// and should be replicated for any Request implementation that doesn't derive from RequestBase.
		/// </summary>
		/// <value><c>true</c> if debugging; otherwise, <c>false</c>.</value>
		public static bool debugging { get { return Requests.debugPinCount > 0; } }

		class DebugBlock : IDisposable
		{
			#region IDisposable implementation
			public void Dispose ()
			{
				Requests.DebugEnd();
			}
			#endregion
		}

		private static int debugPinCount { get; set; }


		/// <summary>
		/// Utility for cases where a service stores the 'active' request, 
		/// e.g. to prevent duplicate concurrent requests.
		/// If the passed request is either queued or in progress, returns the request.
		/// If not, clears the request (this is why it's a ref arg)
		/// </summary>
		public static T ClearIfInactive<T>(ref T r) where T : class, Request
		{
			if(r != null && r.IsQueuedOrInProgress()) {
				return r;
			}
			r = null;
			return null;
		}

	}
}
