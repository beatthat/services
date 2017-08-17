#if BT_LEGACY_WWW
using UnityEngine;
using System;

namespace BeatThat
{
	/// <summary>
	/// TODO: change so that the request can be disposed immediately and the loaded clip disposed elsewhere/later
	/// </summary>

	[Obsolete("use WWWDisposableAudioClipRequest")]public class WWWAudioClipRequest : WWWRequestBase, Request<AudioClip>
	{
		public enum Mode { COMPRESSED = 0, UNCOMPRESSED = 1, STREAM = 2 }

		public WWWAudioClipRequest(WWWRequestRunner runner, string url = null, Mode mode = Mode.COMPRESSED, bool load3D = false, float delay = 0f) : base(runner, url, delay) 
		{
			this.mode = mode;
			this.load3D = load3D;
		}

		virtual public void Execute(Action<Request<AudioClip>> callback) 
		{
			if(callback == null) {
				Execute();
				return;
			}

			RequestExecutionPool<AudioClip>.Get().Execute(this, callback);
		}

		public object GetItem() { return this.item; } 
		public AudioClip item { get; protected set; }
		public bool load3D { get; set; }
		public Mode mode { get; set; }

		override protected void DoOnError()
		{
			if(this.www == null) {
				return;
			}

			var err = www.GetJsonError(); // TODO: try to make error details come from response headers
			
			Debug.LogError("Failed to retrieve item from url '" + this.www.url + "', error='" + err
			               + "', req.error='" + this.www.error + "'");
			
			this.error = err;
		}

		override protected void AfterDisposeWWW()
		{
			if(this.item == null) {
				return;
			}

//			Debug.LogError("[" + Time.frameCount + "] " + GetType() + "::DoDispose " + this.url);

			UnityEngine.Object.DestroyImmediate(this.item);

			this.item = null;
		}

		override protected void DoOnDone()
		{
			switch(this.mode) {
			case Mode.COMPRESSED:
				this.item = this.www.GetAudioClipCompressed(this.load3D);
				break;
			case Mode.UNCOMPRESSED:
				this.item = this.www.GetAudioClip(this.load3D, false);
				break;
			case Mode.STREAM:
				this.item = this.www.GetAudioClip(this.load3D, true);
				break;
			}

			if(string.IsNullOrEmpty(this.item.name)) {
				this.item.name = this.url;
			}
		}
	}
}
#endif