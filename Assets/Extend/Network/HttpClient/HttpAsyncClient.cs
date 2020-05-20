using System.Net;
using System.Text;
using Extend.Common.Lua;
using XLua;

namespace Extend.Network.HttpClient {
	[LuaCallCSharp]
	public class HttpAsyncClient {
		private HttpWebRequest request;
		private byte[] responseBuffer = new byte[2048];
		public static HttpAsyncClient Create(string url, string method, int timeout) {
			var req = WebRequest.CreateHttp(url);
			var client = new HttpAsyncClient {
				request = req
			};
			req.Method = method;
			req.Timeout = timeout;
			req.ContentType = "application/json; charset=utf-8";
			return client;
		}

		public async void DoJsonRequest(string json, LuaFunction callback) {
			if( request.Method != "GET" ) {
				var stream = await request.GetRequestStreamAsync();
				var buffer = Encoding.UTF8.GetBytes(json);
				await stream.WriteAsync(buffer, 0, buffer.Length);
				await stream.FlushAsync();
			}

			var response = await request.GetResponseAsync();
			var length = response.ContentLength;
			ExtendBuffer(length);
			var responseStream = response.GetResponseStream();
			var readCount = await responseStream.ReadAsync(responseBuffer, 0, responseBuffer.Length);
			json = Encoding.UTF8.GetString(responseBuffer, 0, readCount);
			callback.Call(json);
		}

		private static int MinPo2(long length) {
			var pow = 1;
			while( length > 0 ) {
				pow <<= 1;
				length >>= 1;
			}

			return pow;
		}

		private void ExtendBuffer(long length) {
			if( responseBuffer.Length < length ) {
				length = MinPo2(length);
				responseBuffer = new byte[length];
			}
		}
	}
}