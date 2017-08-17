#if BT_LEGACY_WWW
using UnityEngine;

namespace BeatThat
{

	public class WWWRequestBase : RequestBase, WWWRequest 
	{
		public WWWRequestBase(WWWRequestRunner runner, string url = null, float delay = 0f)
		{
			this.runner = runner;
			this.url = url;
			this.delay = delay;
		}

		#region ServiceRequest implementation

		/// <summary>
		/// Ensures that url (WWWForm) form are ready to send.
		/// Base implementation calls the action 'prerpareDelegate' (if it is not null);
		/// </summary>
		virtual public void Prepare() 
		{
			if(this.prepareDelegate != null) {
				this.prepareDelegate(this);
			}
		}

		public System.Action<WWWRequestBase> prepareDelegate { get; set; }

		override public float progress 
		{
			get {
				switch(this.status) {
				case RequestStatus.IN_PROGRESS:
					if(this.www == null) {
						return 0f;
					}
					return this.www.progress;
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
			PostForm(true).AddField("force", "post"); // dumb WWW has no way to set the method to POST other than adding a form
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

//		override public void Cancel()
//		{
//			Dispose();
//		}

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

		public void OnSent(WWW www)
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

		public void OnDone()
		{
			if(this.status != RequestStatus.IN_PROGRESS) {
				Debug.LogError("[" + Time.frameCount + "] " + GetType() + "::OnDone called in inva status " + this.status + " url=" + this.url);
				return;
			}

			DoOnDone();

			CompleteRequest();

//			UpdateStatus(RequestStatus.DONE);
//
//			Actions.Exec(this.callback);
		}

		virtual protected void DoOnDone() {}

		virtual protected void DoOnError() {}

		protected WWW www { get; set; }
//		protected System.Action callback { get; private set; }

		#endregion

		public WWWRequestRunner runner { get; private set; }

	}
}
#endif