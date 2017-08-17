#if BT_LEGACY_WWW
using UnityEngine;
using System;

namespace BeatThat
{
	public static class WWWExtensions 
	{

		public static int GetResponseCode(this WWW req)
		{
			string status;
			if(!req.responseHeaders.TryGetValue("STATUS", out status)) {
				Debug.LogWarning("No STATUS header in response to url '" + req.url + "'");
				return 500;
			}
			else {
#pragma warning disable 0168
				try {
					return int.Parse(status.Split(' ')[1]);
				}
				catch(System.Exception e) {
					Debug.LogWarning("Inva STATUS header in response to url '" + req.url + "': '" + status + "'");
					return 500;
				}
			}
#pragma warning restore 0168
		}

		public static string GetJsonError(this WWW req)
		{
#pragma warning disable 0168
			try {
				Debug.LogError("GetJsonError: url='" + req.url + "' and response='" + req.text + "'");
				ErrorResponse err = null;
				try {
					err = StaticObjectPool<ErrorResponse>.Get();
					JsonUtility.FromJsonOverwrite(req.text, err);
					return err.message;
				}
				finally {
					if(err != null) {
						err.message = null;
						StaticObjectPool<ErrorResponse>.Return(err);
					}
				}
			}
			catch(Exception e) {
				Debug.LogWarning("Unable to parse json error for url '" + req.url + "': body='" + req.text + "'");
				return "error";
			}
#pragma warning restore 0168
		}

		[Serializable]
		public class ErrorResponse
		{
			public string message;
		}
	}
}

#endif