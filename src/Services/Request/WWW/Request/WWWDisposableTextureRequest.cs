#if BT_LEGACY_WWW
using UnityEngine;
using System;

namespace BeatThat
{
	/// <summary>
	/// TODO: change so that the request can be disposed immediately and the loaded clip disposed elsewhere/later
	/// </summary>
	public class WWWDisposableTextureRequest : WWWRequestBase, Request<Disposable<Texture2D>>
	{
		public WWWDisposableTextureRequest(WWWRequestRunner runner, string url = null, float delay = 0f) : base(runner, url, delay) {}

		public object GetItem() { return this.item; } 
		public Disposable<Texture2D> item { get; protected set; }

		virtual public void Execute(Action<Request<Disposable<Texture2D>>> callback) 
		{
			if(callback == null) {
				Execute();
				return;
			}

			RequestExecutionPool<Disposable<Texture2D>>.Get().Execute(this, callback);
		}

		override protected void DoOnError()
		{
			if(this.www == null) {
				return;
			}

			var err = this.www.GetJsonError();
			
			Debug.LogError("Failed to retrieve item from url '" + this.www.url + "', error='" + err
			               + "', req.error='" + this.www.error + "'");
			
			this.error = err;
		}

		override protected void DoOnDone()
		{
			this.item = new DownloadedAssetDisposable<Texture2D>(this.www.textureNonReadable);
		}
	}
}
#endif