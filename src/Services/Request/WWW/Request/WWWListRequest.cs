#if BT_LEGACY_WWW
using UnityEngine;
using BeatThat;
using System.IO;
using BeatThat.Serialization;
using System;

namespace BeatThat
{
	public class WWWListRequest<T> : WWWRequestBase, ListRequest<T> where T : class
	{
		public WWWListRequest(Reader<T> format, WWWRequestRunner runner, string url = null, float delay = 0f) : base(runner, url, delay)
		{
			this.format = format;
		}

		public T[] items { get; protected set; }
		public Reader<T> format { get; set; }

		virtual public void Execute(Action<ListRequest<T>> callback) 
		{
			if(callback == null) {
				Execute();
				return;
			}

			ListRequestExecutionPool<T>.Get().Execute(this, callback);
		}

		override protected void DoOnError()
		{
			if(this.www == null) {
				return;
			}
			
			var err = this.www.GetJsonError();
			
			Debug.LogError("Failed to retrieve list from url '" + this.www.url + "', error=" + err
			               + ", req.error='" + this.www.error);
			
			this.error = err;
		}

		override protected void AfterDisposeWWW()
		{
			if(this.items == null) {
				return;
			}

			if(typeof(UnityEngine.Object).IsAssignableFrom(typeof(T))) {
				foreach(var i in this.items) {
					var o = i as UnityEngine.Object;
					if(o != null) {
						UnityEngine.Object.DestroyImmediate(o);
					}
				}
			}
			this.items = null;
		}


		override protected void DoOnDone()
		{
			try {
				this.items = this.format.ReadArray(new MemoryStream(www.bytes));
			}
			catch(Exception e) {
				Debug.LogError("Failed to parse results. url=" + this.www.url 
					+ ", response=" + this.www.text + ", with format " + this.format.GetType().Name + " error=" + e.Message);

				this.error = "format";
			}
		}
	}
}
#endif