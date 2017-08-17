using UnityEngine;
using System;

namespace BeatThat
{
	
	/// <summary>
	/// Useful for when you want to create an adhoc request from (a series of) async calls.
	/// 
	/// Similar to js Promise in the sense that it works by its constructor taking callback function for Resolve(item) and Reject(err).
	/// 
	/// NOT like Promise in the sense that execution is deferred (until Request::Execute is called)
	/// and does not currently support .then continuation syntax.
	/// 
	/// PromiseRequest is convenient because it can be used in places where you'd otherwise need to create a new Request implementation.
	/// It does have some issues though:
	/// 
	/// 1) Generally glosses over handling of Request::Cancel (no mechanism provided to cancel internal execution if the Promise itself gets cancelled)
	/// 
	/// 2) (For the hyper allocation sensitive) Virtually impossible to use without creating allocating closures, 
	/// although most situations where you'd use a Request there are going to be lots of allocations anyway.
	/// 
	/// 
	/// </summary>
	public class Promise<T> : RequestBase, Request<T>, HasResponseCode
	{
		#region HasResponseCode implementation
		/// <summary>
		/// A convenience since it is so common to make web calls from inside a request.
		/// In the default implementation, when reject(err) is called with an err object that implements HasResponseCode,
		/// that response code will be returned here.
		/// </summary>
		/// <returns>The response code.</returns>
		public long GetResponseCode ()
		{
			return this.responseCode;
		}
		#endregion
		private long responseCode { get; set; }

		/// <summary>
		/// A delegate function that executes the request. 
		/// MUST terminate by calling one of the passed in callback functions, resolve or reject
		/// 
		/// @param resolve - the outer action should call resolve with the request's result item (if the request is successful)	
		/// 
		/// @param reject - the outer action should call reject with an error message (if the request is a failure).
		/// 	The param type is object rather than string to enable support for secondary error info, e.g. web response codes.
		/// 
		/// @param cancel - the outer action should call cancel if, say, an internal request made by the promise is cancelled
		/// 
		/// @param attach - adds a form of cancel support to Promise.
		/// 	 Any attached disposible will be disposed when the promise ends (including by cancel)
		/// 
		/// </summary>
		public delegate void ExecWithResolveOrReject(Action<T> resolve, Action<object> reject, Action cancel, Action<IDisposable> attach);

		/// <summary>
		/// Creates a PromiseRequest similar to the JS model.
		/// 
		/// You would generally use a promise if you have an action/command that returns a Request
		/// but internally issues a sequence of Request *or* delegates a request 
		/// and then wants to handle the results internally before dispatching the callback.
		/// 
		/// Typical execution boilerplate looks like this:
		/// 
		/// <code>
		/// public Request Foo(Action<Request> callback) {
		/// 	var p = new Promise((resolve, reject, cancel, attach) => {
		/// 		
		/// 		// var api = ... we're calling some other API
		/// 		
		/// 		var subReq = api.Fetch((itemRequest) => {
		/// 			if(itemRequest.isCancelled) {
		/// 				cancel();
		/// 				return;
		/// 			}
		/// 
		/// 			if(itemRequest.hasError) {
		/// 				// handle error
		/// 				reject(itemRequest.error);
		/// 				return;
		/// 			}
		/// 			
		/// 			// when we get the API response, we want to process it here
		/// 			// that's why we're using a promise
		/// 
		/// 			this.storedItem = itemRequest.item;
		/// 
		/// 			resolve(itemRequest.item);
		/// 		});
		/// 
		/// 		attach(subReq); // this way if the Promise::Cancel is called, the subrequest will also be disposed
		///		});
		/// 	
		/// 	p.Execute(callback);
		/// 	return p;
		/// </code>
		/// </summary>
		/// <param name="execDel">Exec del.</param>
		public Promise(ExecWithResolveOrReject execDel)
		{
			this.execDelegate = execDel;
		}

		public object GetItem () { return this.item; }
		public T item { get; private set; }

		protected override void ExecuteRequest ()
		{
			UpdateToInProgress();
			this.execDelegate(this.OnResolve, this.OnReject, this.Cancel, this.OnAttach);
		}

		virtual protected void OnAttach(IDisposable d)
		{
			if(this.status == RequestStatus.DONE) {
				d.Dispose();
				return;
			}

			if(this.attached == null) {
				this.attached = ListPool<IDisposable>.Get();
			}
			this.attached.Add(d);
		}

		virtual protected void OnResolve(T item)
		{
			if(this.isCancelled) {
				Debug.LogWarning("[" + Time.frameCount + "] " + GetType() + " resolve called after wrapper promise request was cancelled");
				return;
			}
			this.item = item;
			CompleteRequest();
		}

		virtual protected void OnReject(object errMsgOrRequest)
		{
			if(this.isCancelled) {
				Debug.LogWarning("[" + Time.frameCount + "] " + GetType() + " reject called after wrapper promise request was cancelled");
				return;
			}

			var asString = errMsgOrRequest as string;
			if(asString != null) {
				CompleteWithError(asString);
				return;
			}

			var hasCode = errMsgOrRequest as HasResponseCode;
			if(hasCode != null) {
				this.responseCode = hasCode.GetResponseCode();
			}

			var asReq = errMsgOrRequest as Request;
			if(asReq != null) {
				CompleteWithError(asReq.error);
				return;
			}

			CompleteWithError(errMsgOrRequest != null? errMsgOrRequest.ToString(): "Error");
		}

		sealed override protected void BeforeCompletionCallback() 
		{
			this.responseCode = this.DetermineResponseCode();

			var a = this.attached;
			this.attached = null;

			if(a != null) {
				foreach(var d in a) {
					if(d != null) { d.Dispose(); }
				}
				ListPool<IDisposable>.Return(a);
			}

			BeforePromiseCompletionCallback();
		}

		virtual protected void BeforePromiseCompletionCallback() {}

		virtual public void Execute(Action<Request<T>> callback) 
		{
			if(callback == null) {
				Execute();
				return;
			}

			RequestExecutionPool<T>.Get().Execute(this, callback);
		}

		protected ExecWithResolveOrReject execDelegate { get; set; }
		protected ListPoolList<IDisposable> attached { get; set; }
	}

	public class Promise : Promise<object> 
	{
		public Promise(ExecWithResolveOrReject execDel) : base(execDel) {}
	}

	public static class PromiseRequest
	{
		/// <summary>
		/// Boilerplate handler for error and cancel results returned on subrequest made by a Promise.
		/// </summary>
		public static bool HandleErrorOrCancel(Request r, Action<object> reject, Action cancel)
		{
			if(r.isCancelled) {
				cancel();
				return true;
			}

			if(r.hasError) {
				reject(r.error);
				return true;
			}

			return false;
		}
	}


}