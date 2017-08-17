using System.Collections.Generic;
using UnityEngine;

namespace BeatThat
{
	public class ChainRequests : RequestBase
	{
		public ChainRequests Add(Request r)
		{
			m_requests.Add(r);
			return this;
		}

		#region implemented abstract members of ServiceRequestBase

		/// <summary>
		/// For now treats all subrequests as representing equal shares of progress
		/// </summary>
		/// <value>The progress.</value>
		override public float progress 
		{ 
			get { 
				var n = (float)m_requests.Count;

				var completed = (this.curRequestIndex / n);

				var curReq = this.curRequest;
				if(curReq == null) {
					return completed;
				}

				return completed + (curReq.progress / n);
			}
		}

		protected override void DisposeRequest ()
		{
			foreach(var r in m_requests) {
				r.Dispose();
			}
			m_requests.Clear();
		}

		override protected void AfterCompletionCallback()
		{
			m_requests.Clear();
		}

		protected override void ExecuteRequest ()
		{
			this.curRequestIndex = -1;
			UpdateToInProgress();
			ExecuteNext();
		}

		#endregion

		private void ExecuteNext()
		{
			this.curRequestIndex++;

			if(this.curRequestIndex >= m_requests.Count) {
				CompleteRequest(RequestStatus.DONE);
				return;
			}

			var req = m_requests[this.curRequestIndex];

			switch(req.status) {
			case RequestStatus.DONE:
				if(req.hasError) {
					CompleteWithError(req.error);
					return;
				}
				ExecuteNext();
				return;

			case RequestStatus.CANCELLED:
				Debug.LogWarning("subrequest cancelled");
				Cancel();
				return;

			case RequestStatus.QUEUED:
			case RequestStatus.IN_PROGRESS: // already Executed

//				Debug.LogError("[" + Time.frameCount + "] " + GetType() + "::ExecuteNext will proxy request " + req + " in status " + req.status);
				req = new ProxyRequest(req); // TODO: pool?
				break;
			}

			req.Execute(() => {
				if(req.status != RequestStatus.DONE || !string.IsNullOrEmpty(req.error)) { // TODO: always fails on subrequest error, should FAIL_ON_ERROR be a default option w override
					CompleteWithError(req.error);
					return;
				}
				ExecuteNext();
			});
		}

		private int curRequestIndex { get; set; }

		private Request curRequest 
		{
			get {
				if(this.curRequestIndex < 0 || this.curRequestIndex >= m_requests.Count) {
					return null;
				}
				return m_requests[this.curRequestIndex];
			}
		}

		private readonly List<Request> m_requests = new List<Request>();
	}
}
