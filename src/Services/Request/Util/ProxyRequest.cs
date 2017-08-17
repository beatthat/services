using System;
using UnityEngine;

namespace BeatThat
{
	public class ProxyRequest : RequestBase, HasResponseCode
	{
		public ProxyRequest(Request r)
		{
			this.request = r;
		}

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

		override public float progress  { get { return this.request != null? this.request.progress: 0f; } }

		public void ReplaceProxiedRequest(Request proxied) 
		{
			this.request = proxied;
			OnRequestStatusUpdated();
		}

		protected override void DisposeRequest()
		{
			if(this.request == null) {
				return;
			}
			Cleanup();
			this.request.Dispose();
			this.request = null;
		}
	
		protected override void ExecuteRequest()
		{
			UpdateToInProgress();

			this.ignoreRequestStatusUpdates = true;

			if(this.request.status == RequestStatus.NONE) {
				this.request.Execute();
			}

			this.request.StatusUpdated += this.requestStatusUpdatedAction;

			this.ignoreRequestStatusUpdates = false;

			OnRequestStatusUpdated();
		}

		sealed override protected void BeforeCompletionCallback() 
		{
			this.responseCode = this.DetermineResponseCode();
			BeforeProxyCompletionCallback();
		}

		virtual protected void BeforeProxyCompletionCallback() {}


		sealed override protected void AfterCompletionCallback()
		{
			AfterProxyCompletionCallback();
			Cleanup();
		}

		virtual protected void AfterProxyCompletionCallback() {}

		private void Cleanup()
		{
			if(this.request == null) {
				return;
			}
			this.request.StatusUpdated -= this.OnRequestStatusUpdated;
		}

		private bool ignoreRequestStatusUpdates { get; set; }

		private void OnRequestStatusUpdated()
		{
			if(this.ignoreRequestStatusUpdates) {
				return;
			}

			if(this.request.hasError) {
				CompleteWithError(this.request.error);
				return;
			}

			if(this.request.status == RequestStatus.DONE) {
				CompleteRequest(RequestStatus.DONE);
				return;
			}

			if(this.request.isCancelled) {
				Cancel();
				return;
			}

			if(!this.request.IsQueuedOrInProgress()) {
				#if BT_DEBUG_UNSTRIP
				Debug.LogWarning("[" + Time.frameCount + "] " + GetType() + "::OnRequestStatusUpdated status=" + this.request.status + " and CANCELLING");
				#endif
				Cancel();
			}
		}
		private Action requestStatusUpdatedAction { get { return m_requestStatusUpdatedAction?? (m_requestStatusUpdatedAction = this.OnRequestStatusUpdated); } }
		private Action m_requestStatusUpdatedAction;

		protected Request request { get; private set; }
	}

	public class ProxyRequest<T> : ProxyRequest, Request<T>
	{
		public ProxyRequest(Request<T> r) : base(r) {}

		public object GetItem() { return this.item; } 
		public T item { get { return this.request != null? (this.request as Request<T>).item: default(T); } }
		
	}
}
