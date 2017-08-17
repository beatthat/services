using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;

namespace BeatThat
{
	public class WebRequestBase : RequestBase, WebRequest 
	{
		
		public WebRequestBase(WebRequestRunner runner, string url, HttpVerb httpVerb = HttpVerb.GET, float delay = 0f)
		{
			this.runner = runner;
			this.url = url;
			this.delay = delay;
			this.httpVerb = httpVerb;
		}

		public HttpVerb httpVerb { get; protected set; }

		public long GetResponseCode()
		{
			return this.www != null? this.www.responseCode: 0L;
		}

		public string GetResponseText()
		{
			return this.www != null && this.www.isDone? this.www.downloadHandler.text: null;
		}
			
		/// <summary>
		/// Ensures that url (WWWForm) form are ready to send.
		/// Base implementation calls the action 'prepareDelegate' (if it is not null);
		/// </summary>
		virtual public void Prepare() 
		{
			if(this.prepareDelegate != null) {
				this.prepareDelegate(this);
			}

			switch(this.httpVerb) {
			case HttpVerb.GET:
				this.www = UnityWebRequest.Get(this.url);
				break;
			case HttpVerb.POST:
				if(this.form == null) {
					ForcePost(); // TODO: shouldn't need a form to POST
				}
				this.www = UnityWebRequest.Post (this.url, this.form);
				break;
			default:
				throw new NotSupportedException(this.httpVerb.ToString()); // add later
			}

			if(m_headers != null) {
				foreach(var h in m_headers) {
					this.www.SetRequestHeader(h.Key, h.Value);
				}
			}
		}

		public Action<WebRequestBase> prepareDelegate { get; set; }

		public void SetHeader(string name, string value)
		{
			if(m_headers == null) {
				m_headers = new Dictionary<string, string>(); // TODO: pool
			}
			m_headers[name] = value;
		}
		private IDictionary<string,string> m_headers;

		override public float progress 
		{
			get {
				switch(this.status) {
				case RequestStatus.IN_PROGRESS:
					if(this.www == null) {
						return 0f;
					}
					return this.www.downloadProgress;
				case RequestStatus.DONE:
					return 1f;
				default:
					return 0f;
				}
			}
		}

		override public float uploadProgress 
		{
			get {
				switch(this.status) {
				case RequestStatus.IN_PROGRESS:
					if(this.www == null) {
						return 0f;
					}
					return this.www.uploadProgress;
				case RequestStatus.DONE:
					return 1f;
				default:
					return 0f;
				}
			}
		}

		public float delay { get; set; }
		public string url { get; set; }
		public WWWForm form { get; set; }

		public void ForcePost()
		{
			PostForm(true).AddField("force", "post"); // TODO: figure out how UnityWebRequest wants you to send an empty post
		}

		public void AddPostField(string name, string value)
		{
			PostForm(true).AddField(name, value);
		}

		public void AddPostBinaryField(string name, byte[] value, string fileName = null, string mimeType = null)
		{
			PostForm(true).AddBinaryData(name, value, fileName, mimeType);
		}

		protected WWWForm PostForm(bool create)
		{
			if(this.form == null && create) {
				this.form = new WWWForm();
			}
			return this.form;
		}

		sealed override protected void DisposeRequest()
		{
//			Debug.LogWarning("[" + Time.frameCount + "] " + GetType() + "::Dispose url='" + this.url + "'");

			if(this.www != null) {
				this.www.Dispose();
				this.www = null;
			}
			AfterDisposeWWW();
		}

		virtual protected void AfterDisposeWWW()
		{
		}

		override protected void ExecuteRequest()//System.Action callback = null)
		{
			this.runner.Execute(this);
		}

		public void OnQueued()
		{
			UpdateToQueued();
		}

		public void OnSent()
		{
			this.www = www;
			UpdateToInProgress();
			DoOnSent();
		}

		virtual protected void DoOnSent()
		{
		}

		public void OnError(string err)
		{
			this.error = err;

			DoOnError();

			if(!this.hasError) {
				// DoOnError might have decided there is no error after all
				CompleteRequest();
				return;
			}
				
			CompleteWithError(this.error);
		}

		public bool isNetworkError 
		{ 
			get {
				#if UNITY_2017_OR_NEWER
				return this.www.isNetworkError;
				#else
				// terrible hack
				return this.www != null && !string.IsNullOrEmpty(this.www.error) && this.www.error.IndexOf("No Internet Connection") >= 0; 
				#endif
			}
		}


		public void OnDone()
		{
			if(this.status != RequestStatus.IN_PROGRESS) {
				Debug.LogError("[" + Time.frameCount + "] " + GetType() + "::OnDone called in inva status " + this.status + " url=" + this.url);
				return;
			}

			DoOnDone();

			CompleteRequest();
		}

		virtual protected void DoOnDone() {}

		virtual protected void DoOnError() {}

		public UnityWebRequest www { get; protected set; }

		public WebRequestRunner runner { get; private set; }

	}
}
