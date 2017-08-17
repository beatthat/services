#if BT_LEGACY_WWW
using UnityEngine;
using System.Collections;
using BeatThat.Service;

namespace BeatThat
{
	
	/// <summary>
	/// TODO: make all (or most) WWWRequest's safe to dispose immediately following completion. Will require different handling of downloaded textures, audioclips, etc.
	/// </summary>
	[RegisterService(typeof(WWWRequestRunner))]
	public class DefaultWWWRequestRunner : MonoBehaviour, WWWRequestRunner 
	{
		public static bool LOG_REQUESTS = true;

		public void Execute(WWWRequest req)
		{
			req.OnQueued();
			StartCoroutine(DoExecute(req));
		}

		private IEnumerator DoExecute(WWWRequest req)
		{
			if(req.delay > 0f) {
				yield return new WaitForSeconds(req.delay);
			}

			if(req.status == RequestStatus.CANCELLED || req.status == RequestStatus.DONE) {
				yield break;
			}

			req.Prepare();

			var url = req.url;
			var form = req.form;

			var www = (form != null)? new WWW(url, form): new WWW(url);

			if(LOG_REQUESTS) {
				Debug.Log("[" + Time.frameCount + "] " + GetType() + " executing request '" + www.url + "'");
			}

			req.OnSent(www);

#if UNITY_IOS
			// Cannot be interrupted by WWW.Dispose() on iOS
			while (req.status == RequestStatus.IN_PROGRESS && !www.isDone) { 
				yield return new WaitForEndOfFrame();
			}
#else
			yield return www; 
#endif

			if(req.status == RequestStatus.CANCELLED) {
				yield break;
			}

			if(!www.isDone || !string.IsNullOrEmpty(www.error)) {
				req.OnError(www.error);
			}
			else {
				req.OnDone();
			}
		}
	}
}
#endif