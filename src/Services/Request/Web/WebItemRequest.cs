using UnityEngine;
using System.IO;
using BeatThat.Serialization;
using System;

namespace BeatThat
{
	public class WebItemRequest<T> : WebRequestBase, Request<T> 
	{
		public WebItemRequest(Reader<T> format, WebRequestRunner runner, string url, HttpVerb httpVerb = HttpVerb.GET, float delay = 0f) : base(runner, url, httpVerb, delay)
		{
			this.format = format;
			this.itemReader = format.itemReader;
		}

		public WebItemRequest(ReadItemDelegate<T> itemReader, WebRequestRunner runner, string url, HttpVerb httpVerb = HttpVerb.GET, float delay = 0f) : base(runner, url, httpVerb, delay)
		{
			this.itemReader = itemReader;
		}

		public object GetItem() { return this.item; } 
		public T item { get; protected set; }
		private Reader<T> format { get; set; }
		private ReadItemDelegate<T> itemReader { get; set; }

		virtual public void Execute(Action<Request<T>> callback) 
		{
			if(callback == null) {
				Execute();
				return;
			}

			RequestExecutionPool<T>.Get().Execute(this, callback);
		}

//		override protected void DoOnError()
//		{
//			if(this.www == null) {
//				return;
//			}
//			
//			var err = this.www.GetJsonError();
//			
//			Debug.LogError("Failed to retrieve item from url '" + this.www.url + "', error='" + err
//			               + "', req.error='" + this.www.error + "'");
//			
//			this.error = err;
//		}

		sealed override protected void AfterDisposeWWW()
		{
			var disposeFormat = this.format as IDisposable;
			if(disposeFormat != null) {
				disposeFormat.Dispose();
			}

			this.format = null;
			this.itemReader = null;

			// TODO: this is wrong. Dispose should dispose request resources (e.g. buffers) but not be the means to destroy a loaded item
			var obj = this.item as UnityEngine.Object;
			this.item = default(T);
			if(obj != null) {
				UnityEngine.Object.DestroyImmediate(obj);
			}

			this.item = default(T);

			AfterDisposeItem();
		}

		virtual protected void AfterDisposeItem() {}

		override protected void DoOnDone()
		{
			ReadItem();	
		}

		protected bool ReadItem()
		{
			try {
				using(var s = new MemoryStream(this.www.downloadHandler.data)) {
					this.item = this.itemReader(s);
				}

				return true;
			}
			catch(Exception e) {
				Debug.LogError("Failed to parse item results: url=" + this.www.url 
					+ ", response=" + this.www.downloadHandler.text + ", error=" + e.Message);

				this.error = "format";

				#if UNITY_EDITOR
				throw e;
				#else 
				return false;
				#endif
			}
		}
	}
}