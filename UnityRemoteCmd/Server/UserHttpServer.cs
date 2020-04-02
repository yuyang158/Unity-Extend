using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server {
	public class UserHttpServer {
		private readonly HttpListener listener;
		private const int PORT = 4100;

		private readonly Dictionary<string, Func<HttpListenerContext, string, string>> routers =
			new Dictionary<string, Func<HttpListenerContext, string, string>>();

		public UserHttpServer() {
			listener = new HttpListener {
				AuthenticationSchemes = AuthenticationSchemes.Anonymous
			};
			listener.Prefixes.Add($"http://*:{PORT}/");
			listener.Start();
			Console.WriteLine($"HTTP Server started : {PORT}");
#pragma warning disable 4014
			Start();
#pragma warning restore 4014
		}

		private static async Task<string> LoadContent(Stream stream, long size) {
			var body = new byte[size];
			var readCount = 0;
			while( readCount < size ) {
				var count = await stream.ReadAsync(body, readCount, body.Length - readCount);
				readCount += count;
			}

			return Encoding.UTF8.GetString(body);
		}

		private async Task Start() {
			while( true ) {
				try {
					var context = await listener.GetContextAsync();
					var req = context.Request;
					var res = context.Response;
					var body = await LoadContent(req.InputStream, req.ContentLength64);
					var cmd = req.QueryString["cmd"];
					if( !routers.TryGetValue(cmd, out var func) ) {
						res.StatusCode = 404;
						res.Close();
						return;
					}
				
					var response = func(context, body);
					Console.WriteLine($"cmd {cmd} --> response {response}");

					res.ContentEncoding = Encoding.UTF8;
					await res.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(response));
					res.Close();
				}
				catch( Exception e ) {
					Console.WriteLine(e);
				}
			}
		}

		public void RegisterRouter(string path, Func<HttpListenerContext, string, string> func) {
			routers.Add(path, func);
		}
	}
}