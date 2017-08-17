using UnityEngine;
using System.Collections;
using BeatThat.Serialization;
using System.IO;
using System;

namespace BeatThat
{
	public class FormattedResourceItemRequest<T> : RequestBase, Request<T> where T : class
	{
		public FormattedResourceItemRequest(string path, Reader<T> format) 
		{
			this.path = path;
			this.format = format;
		}

		virtual public void Execute(Action<Request<T>> callback) 
		{
			if(callback == null) {
				Execute();
				return;
			}

			RequestExecutionPool<T>.Get().Execute(this, callback);
		}

		public object GetItem() { return this.item; } 
		public T item { get; private set; }
		public string path { get; private set; }
		
		private ResourceRequest req { get; set; }
//		private System.Action callback { get; set; }
		private GameObject runner { get; set; }
		private Reader<T> format { get; set; }

		#region ServiceRequest implementation

		override public float progress 
		{
			get {
				switch(this.status) {
				case RequestStatus.IN_PROGRESS:
					if(this.req == null) {
						return 0f;
					}
					return this.req.progress;
				case RequestStatus.DONE:
					return 1f;
				default:
					return 0f;
				}
			}
		}

//		override public void Cancel()
//		{
//			Dispose();
//		}

		override protected void DisposeRequest()
		{
			if(this.runner != null) {
				UnityEngine.Object.DestroyImmediate(this.runner);
			}

			this.item = null;
//			UpdateStatus(RequestStatus.CANCELLED);

//			Object obj = this.item as Object;
//			this.item = null;
//			// only unload if the object type is a file asset, e.g. a texure or audio file
//			if(obj != null && !(obj is GameObject) && !(obj is Component)) {
//				Resources.UnloadAsset(obj);
//			}
		}

		override protected void ExecuteRequest() //(System.Action callback = null)
		{
//			this.callback = callback;

			this.runner = new GameObject("ResourceLoader-" + this.path);
			this.runner.AddComponent<MonoBehaviour>().StartCoroutine(RunRequest());
		}

		#endregion

		private IEnumerator RunRequest()
		{
			UpdateStatus(RequestStatus.IN_PROGRESS);
			
			this.req = Resources.LoadAsync<TextAsset>(this.path);
			
			yield return this.req;
			
			if(req.asset == null) {
				CompleteWithError("Failed to load resource at path '" + this.path + "'");
				yield break;
			}

			var text = (this.req.asset as TextAsset);

			try {
				using(var s = new MemoryStream(text.bytes)) {
					this.item = this.format.ReadOne(s);
				}
			}
			catch(Exception e) {
				Debug.LogError("Failed to parse item results: path=" + this.path
					+ ", response=" + text.text + ", error=" + e.Message);

				CompleteWithError("format");
				yield break;
//				this.error = "format";
			}

			Resources.UnloadAsset(this.req.asset);
			this.req = null;
			
			if(this.item == null) {
				CompleteWithError("Failed to cast resource at path '" + this.path + "' to type " + typeof(T));
				yield break;
			}
			
			CompleteRequest();//RequestStatus.DONE);
		}
		
//		private void Finish(RequestStatus s)
//		{
//			UpdateStatus(s);
//			if(this.callback != null) {
//				this.callback();
//			}
//		}
//
//		private void FinishWError(string error)
//		{
//			Debug.LogWarning(error);
//			this.error = error;
//			Finish(RequestStatus.DONE);
//		}


	}
}
