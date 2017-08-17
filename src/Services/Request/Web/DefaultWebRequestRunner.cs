using UnityEngine;
using System.Collections;
using BeatThat.Service;

namespace BeatThat
{
	
	/// <summary>
	/// TODO: make all (or most) WWWRequest's safe to dispose immediately following completion. Will require different handling of downloaded textures, audioclips, etc.
	/// </summary>
	[RegisterService(typeof(WebRequestRunner))]
	public class DefaultWebRequestRunner : MonoBehaviour, WebRequestRunner 
	{
		public bool m_logSend = 
			#if BT_WEBREQUEST_LOG_SEND || UNITY_EDITOR
			true;
			#else
			false;
			#endif

		public bool m_logCompleted = 
			#if BT_WEBREQUEST_LOG_COMPLETE || UNITY_EDITOR
			true;
			#else
			false;
			#endif

		public bool m_disableLogError = 
			#if BT_WEBREQUEST_DISABLE_LOG_ERROR 
			true;
			#else
			false;
			#endif


		public void Execute(WebRequest req)
		{
			req.OnQueued();
			StartCoroutine(DoExecute(req));
		}

		private IEnumerator DoExecute(WebRequest req)
		{
			if(req.delay > 0f) {
				yield return new WaitForSeconds(req.delay);
			}

			if(req.status == RequestStatus.CANCELLED || req.status == RequestStatus.DONE) {
				yield break;
			}

			req.Prepare();

			var www = req.www;

			#pragma warning disable 219
			var token = www.Send();
			#pragma warning restore 219

			var timeStart = Time.realtimeSinceStartup;

			if(m_logSend) {
				Debug.Log("[" + Time.frameCount + "] " + GetType() + " executing " + www.method + " '" + www.url + "'");
			}

			req.OnSent();

#if UNITY_IOS
			// TODO: Cannot be interrupted by WWW.Dispose() on iOS. Need to retest if this is necessary with WebRequest
			while (req.status == RequestStatus.IN_PROGRESS && !www.isDone) { 
				yield return new WaitForEndOfFrame(); // TODO: static
			}
#else
			yield return token; 
#endif

			if(req.status == RequestStatus.CANCELLED) {
				yield break;
			}

			if(!string.IsNullOrEmpty(www.error)) {
				if(!m_disableLogError) {
					Debug.LogError("[" + Time.frameCount + "] " + GetType() + " error executing " + www.method + " '" + www.url + "': " + www.error
						+ " [" + ((Time.realtimeSinceStartup - timeStart) * 1000) + "ms]");
				}
				req.OnError(www.error);
				yield break;
			}

			if(!www.isDone) {
				if(!m_disableLogError) {
					Debug.LogError("[" + Time.frameCount + "] " + GetType() + " req for url failed to complete [" + www.url + "] [" 
						+ ((Time.realtimeSinceStartup - timeStart) * 1000) + "ms]");
				}
				req.OnError("failed to complete request");
				yield break;
			}

			string error;
			if(www.IsError(out error)) {
				req.OnError(error);
				yield break;
			}

			if(m_logCompleted) {
				Debug.Log("[" + Time.frameCount + "] " + GetType() + " COMPLETED " + www.method + " '" + www.url + "' [" + ((Time.realtimeSinceStartup - timeStart) * 1000) + "ms]");
			}
				
			req.OnDone();
		}
	}
}
