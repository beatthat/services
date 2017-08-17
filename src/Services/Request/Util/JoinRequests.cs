using System.Collections.Generic;
using UnityEngine;
using System;

namespace BeatThat
{
	public class JoinRequests : RequestBase
	{
		public JoinRequests()
		{
			m_requests = ListPool<Request>.Get();
			this.failOnAnyError = true;
		}

		public JoinRequests Add(Request r)
		{
			m_requests.Add(r);
			return this;
		}

		/// <summary>
		/// If set TRUE, then fails on the first error encountered.
		/// Default is TRUE
		/// </summary>
		public bool failOnAnyError { get; set; }

		/// <summary>
		/// Returns the progress of the request that is furthest from completion
		/// </summary>
		override public float progress 
		{ 
			get { 
				var minProgress = 1f;
				for(int i = 0; i < m_requests.Count; i++) {
					minProgress = Mathf.Min(m_requests[i].progress, minProgress);
				}
				return minProgress;
			}
		}

		public int count { get { return m_requests != null? m_requests.Count: 0; } }

		protected override void DisposeRequest()
		{
			this.ignoreSubrequestStatusUpdates = true;
			if(m_requests != null) {
				for(int i = 0; i < m_requests.Count; i++) {
					m_requests[i].Dispose();
				}
			}
			Cleanup();
		}

		protected override void ExecuteRequest()
		{
			UpdateToInProgress();

			this.ignoreSubrequestStatusUpdates = true;

			for(int i = 0; i < m_requests.Count; i++) {
				if(m_requests[i].status == RequestStatus.NONE) {
					m_requests[i].Execute();
				}
				m_requests[i].StatusUpdated += this.subrequestStatusUpdatedAction;
			}

			this.ignoreSubrequestStatusUpdates = false;

			OnSubrequestStatusUpdated();
		}

		public T GetResultItem<T>(int index)
		{
			if(m_requests == null) {
				return default(T);
			}
			if(index < 0 || index > m_requests.Count) {
				return default(T);
			}

			var hasItem = m_requests[index] as ItemRequest;
			return hasItem != null? (T)hasItem.GetItem(): default(T);

		}

		public void GetResultItems<T>(ICollection<T> results)
		{
			if(m_requests == null) {
				return;
			}

			for(int i = 0; i < m_requests.Count; i++) {
				results.Add(GetResultItem<T>(i));
			}
		}

		override protected void AfterCompletionCallback()
		{
			Cleanup(); 
		}

		// TODO: this is probably wrong. Need a distinction between cleanup that gets called after completion and cleanup for dispose.
		// What if a caller wanted to hold on to a completed request?
		private void Cleanup()
		{
			if(m_requests != null) {
				for(int i = m_requests.Count - 1; i >= 0; i--) {
					m_requests[i].StatusUpdated -= this.subrequestStatusUpdatedAction;
				}
				m_requests.Dispose();
				m_requests = null;
			}
		}

		private bool ignoreSubrequestStatusUpdates { get; set; }
		private void OnSubrequestStatusUpdated()
		{
			if(this.ignoreSubrequestStatusUpdates) {
				return;
			}

//			Debug.LogWarning("[" + Time.frameCount + "]" + GetType() + "::OnSubrequestStatusUpdated  numSubs=" + m_requests.Count);

			bool allDone = true;
			bool anyInProgress = false;
			for(int i = 0; i < m_requests.Count; i++) {

//				Debug.LogWarning("[" + Time.frameCount + "]" + GetType() + "::OnSubrequestStatusUpdated  sub[" + i + "].status=" + m_requests[i].status);

				if(this.failOnAnyError && m_requests[i].hasError) {
					CompleteWithError("Subrequest " + i + " failed with error: " + m_requests[i].error);
					return;
				}

				switch(m_requests[i].status) {
				case RequestStatus.QUEUED:
				case RequestStatus.IN_PROGRESS:
					allDone = false;
					anyInProgress = true;
					break;
				case RequestStatus.DONE:
					break;
				default:
					allDone = false;
					break;
				}
			}

			if(allDone) {
				CompleteRequest(RequestStatus.DONE);
				return;
			}

			if(!anyInProgress) {
				Cancel();
			}
		}
		private Action subrequestStatusUpdatedAction { get { return m_subrequestStatusUpdatedAction?? (m_subrequestStatusUpdatedAction = this.OnSubrequestStatusUpdated); } }
		private Action m_subrequestStatusUpdatedAction;

		private ListPoolList<Request> m_requests;
	}
}
