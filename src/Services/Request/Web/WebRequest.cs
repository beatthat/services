using UnityEngine;
using UnityEngine.Networking;

namespace BeatThat
{
	public enum HttpVerb { GET = 0, POST = 1, HEAD = 3, CREATE = 4, PUT = 5, DELETE = 5 }

	public interface WebRequest : NetworkRequest, HasResponseCode, HasResponseText
	{
		/// <summary>
		/// Ensures that url (WWWForm) form are ready to send
		/// </summary>
		void Prepare();

		string url { get;  }
		WWWForm form { get; set; }

		UnityWebRequest www { get; }

		void SetHeader(string name, string value);

		void OnQueued();

		void OnSent();

		void OnError(string error);

		void OnDone();

		float delay { get; }
	}
}
