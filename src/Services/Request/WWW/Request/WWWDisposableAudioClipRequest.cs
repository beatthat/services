#if BT_LEGACY_WWW
using UnityEngine;
using System;

namespace BeatThat
{
	/// <summary>
	/// TODO: change so that the request can be disposed immediately and the loaded clip disposed elsewhere/later
	/// </summary>
	public class WWWDisposableAudioClipRequest : WWWRequestBase, Request<Disposable<AudioClip>>
	{
		public enum Mode { COMPRESSED = 0, UNCOMPRESSED = 1, STREAM = 2 }

		public WWWDisposableAudioClipRequest(WWWRequestRunner runner, string url = null, Mode mode = Mode.COMPRESSED, bool load3D = false, float delay = 0f) : base(runner, url, delay) 
		{
			this.mode = mode;
			this.load3D = load3D;
		}

		virtual public void Execute(Action<Request<Disposable<AudioClip>>> callback) 
		{
			if(callback == null) {
				Execute();
				return;
			}

			RequestExecutionPool<Disposable<AudioClip>>.Get().Execute(this, callback);
		}

		public object GetItem() { return this.item; } 
		public Disposable<AudioClip> item { get; protected set; }
		public bool load3D { get; set; }
		public Mode mode { get; set; }

		override protected void DoOnError()
		{
			if(this.www == null) {
				return;
			}
			
			var err = this.www.GetJsonError(); // TODO: try to make error details come from response headers
			
			Debug.LogError("Failed to retrieve item from url '" + this.www.url + "', error='" + err
			               + "', req.error='" + this.www.error + "'");
			
			this.error = err;
		}

		override protected void DoOnDone()
		{
			switch(this.mode) {
			case Mode.COMPRESSED:
				this.item = new DownloadedAssetDisposable<AudioClip>(this.www.GetAudioClipCompressed(this.load3D));
				break;
			case Mode.UNCOMPRESSED:
				this.item = new DownloadedAssetDisposable<AudioClip>(this.www.GetAudioClip(this.load3D, false));
				break;
			case Mode.STREAM:
				this.item = new DownloadedAssetDisposable<AudioClip>(this.www.GetAudioClip(this.load3D, true));
				break;
			}

			if(string.IsNullOrEmpty(this.item.item.name)) {
				this.item.item.name = this.url;
			}
		}
	}
}
#endif