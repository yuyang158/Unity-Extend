using UnityEngine;
using UnityEngine.Networking;
using XLua;

namespace Extend.Network.HttpClient {
	[LuaCallCSharp]
	public static class LuaHttpRequest {
		public static void PostJson(string url, string data) {
			UnityWebRequest request = UnityWebRequest.Post(url, data, "application/json");
			var reqOp = request.SendWebRequest();
			reqOp.completed += (operation) => {
				if( request.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError or UnityWebRequest.Result.DataProcessingError ) {
					Debug.LogWarning(request.downloadHandler.text);
				}
				else {
					Debug.Log("Response : " + url);
				}
			};
		}
	}
}