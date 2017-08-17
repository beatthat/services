#if BT_LEGACY_WWW
using UnityEngine;
using System;

namespace BeatThat
{
	/// <summary>
	/// TODO: change so that the request can be disposed immediately and the loaded clip disposed elsewhere/later
	/// </summary>
	public class WWWTextureRequest : WWWRequestBase, Request<Texture2D>
	{
		public WWWTextureRequest(WWWRequestRunner runner, string url = null, float delay = 0f) : base(runner, url, delay) {}

		public object GetItem() { return this.item; } 
		public Texture2D item { get; protected set; }

		virtual public void Execute(Action<Request<Texture2D>> callback) 
		{
			if(callback == null) {
				Execute();
				return;
			}

			RequestExecutionPool<Texture2D>.Get().Execute(this, callback);
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
			this.item = this.www.textureNonReadable;
		}
	}
}
#endif