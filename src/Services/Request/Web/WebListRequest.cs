using UnityEngine;
using BeatThat;
using System.IO;
using BeatThat.Serialization;
using System;

namespace BeatThat
{
	public class WebListRequest<T> : WebRequestBase, ListRequest<T> where T : class
	{
		public WebListRequest(Reader<T> format, WebRequestRunner runner, string url,  HttpVerb httpVerb = HttpVerb.GET, float delay = 0f) : base(runner, url, httpVerb, delay)
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
				this.items = this.format.ReadArray(new MemoryStream(this.www.downloadHandler.data));
			}
			catch(Exception e) {
				Debug.LogError("Failed to parse results. url=" + this.www.url 
					+ ", response=" + this.www.downloadHandler.text + ", with format " + this.format.GetType().Name + " error=" + e.Message);

				this.error = "format";
			}
		}
	}
}
