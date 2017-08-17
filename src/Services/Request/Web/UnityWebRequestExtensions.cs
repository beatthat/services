using UnityEngine;
using System;
using UnityEngine.Networking;

namespace BeatThat
{
	public static class UnityWebRequestExtensions 
	{
		public static bool TryGetJsonError(this UnityWebRequest req, out string error, bool ignoreContentType = false)
		{
			if(!ignoreContentType) {
				var contentType = req.GetResponseHeader("Content-Type");
				if(contentType == null) {
					#if BT_DEBUG_UNSTRIP || UNITY_EDITOR
					Debug.LogError("No content type in response to " + req.url + " body=" + req.downloadHandler.text);
					#endif
					error = null;
					return false;
				}

				if(contentType.IndexOf("application/json") == -1) {
					error = null;
					return false;
				}
			}

			#pragma warning disable 0168
			var text = req.downloadHandler.text;
			try {
				ErrorResponse err = null;
				try {
					err = StaticObjectPool<ErrorResponse>.Get();
					JsonUtility.FromJsonOverwrite(text, err);
					error = err.message;

					return !string.IsNullOrEmpty(error);
				}
				finally {
					if(err != null) {
						err.message = null;
						StaticObjectPool<ErrorResponse>.Return(err);
					}
				}
			}
			catch(Exception e) {
				#if BT_DEBUG_UNSTRIP || UNITY_EDITOR
				Debug.LogWarning("Unable to parse json error for url '" + req.url + "': body='" + text + "'");
				#endif
				error = null;
				return false;
			}
			#pragma warning restore 0168
		}

		public static bool IsError(this UnityWebRequest req, out string error)
		{
			if(req.isError) {
				error = req.error;
				return true;
			}

			var rCode = req.responseCode;
			if(rCode >= 400 && rCode < 500) {
				if(req.TryGetJsonError(out error)) {
					return true;
				}

				switch(rCode) {
				case 400: 
					error = "bad request";
					return true;
				case 401:
					error = "not authorized";
					return true;
				case 403:
					error = "rejected";
					return true;
				case 404:
					error = "not found";
					return true;
				case 405:
					error = "method not allowed";
					return true;
				default:
					error = rCode + " - client error";
					return true;
				}
			}

			if(rCode >= 500 && rCode < 600) {
				if(req.TryGetJsonError(out error)) {
					return true;
				}

				error = rCode + " - server error";
				return true;
			}

			error = null;
			return false;
		}

		[Serializable]
		public class ErrorResponse
		{
			public string message;
		}
	}
}