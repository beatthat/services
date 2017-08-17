#if BT_LEGACY_WWW
using UnityEngine;

namespace BeatThat
{

	public interface WWWRequest : Request 
	{
		/// <summary>
		/// Ensures that url (WWWForm) form are ready to send
		/// </summary>
		void Prepare();

		string url { get;  }
		WWWForm form { get; set; }

		void OnQueued();

		void OnSent(WWW www);

		void OnError(string error);

		void OnDone();

		float delay { get; }
	}
}
#endif