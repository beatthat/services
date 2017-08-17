using System;

namespace BeatThat
{
	public interface Request : IDisposable, HasError
	{
		event Action StatusUpdated;// TODO: replace with UnityEvent

		RequestStatus status { get; }

		bool isCancelled { get; }

		/// <summary>
		/// Progress of the request
		/// </summary>
		float progress { get; } 

		bool hasError { get;  }

//		string error { get; }

		void Cancel();

//		/// <summary>
//		/// Execute the request and when the request completes call the appropriate callback (if provided).
//		/// 
//		/// @param onSuccess - (if provided) called when the request completes without error
//		/// @param onError - (if provided) called when the request completes with error
//		/// @param onCancel - (if provided) called when the request is cancelled
//		/// 
//		/// </summary>
//		void Send(Action onSuccess = null, Action onError = null, Action onCancel = null);

		/// <summary>
		/// Execute the request and call the (optional) callback when the request terminates, successful or otherwise.
		/// </summary>
		void Execute(Action callback = null);

		/// <summary>
		/// Signal to log debug info for a specific request. 
		/// Ususally more useful than a global flag, because there can be so many requests running at a time, 
		/// including multiples of the same type.
		/// </summary>
		bool debug { get; set; }
	}

	public interface ItemRequest
	{
		object GetItem();
	}

	public interface Request<T> : Request, ItemRequest
	{
		T item { get; }
	}

	public static class RequestExtensions
	{
		public static bool IsQueuedOrInProgress(this Request r)
		{
			return r.status == RequestStatus.QUEUED || r.status == RequestStatus.IN_PROGRESS;
		}

		/// <summary>
		/// Execute the request and call the (optional) callback when the request terminates, successful or otherwise.
		/// </summary>
		public static void Execute(this Request r, Action<Request> callback)
		{
			if(callback == null) {
				r.Execute();
				return;
			}
			RequestExecutionPool.Get().Execute(r, callback);
		}

		/// <summary>
		/// Execute the request and call the (optional) callback when the request terminates, successful or otherwise.
		/// </summary>
		public static void Execute<T>(this Request<T> r, Action<Request<T>> callback)
		{
			if(callback == null) {
				r.Execute();
				return;
			}
			RequestExecutionPool<T>.Get().Execute(r, callback);
		}


		/// <summary>
		/// Utility for cases where a service stores the 'active' request, 
		/// e.g. to prevent duplicate concurrent requests.
		/// If the passed request is the same ref as the passed ref, then nulls the ref
		/// If not, clears the request (this is why it's a ref arg)
		/// </summary>
		public static bool ClearIfMatches<T>(this Request r, ref T rRef) where T : class
		{
			if(r == null) {
				return false;
			}
			if(Object.ReferenceEquals(r, rRef)) {
				rRef = null;
				return true;
			}
			return false;
		}
			
	}

}
