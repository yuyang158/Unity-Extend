using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Extend.Common.Editor {
	public static class BuildErrorReport {
		private const string POST_URL = "https://open.feishu.cn/open-apis/bot/v2/hook/7edd60ad-1e5e-4c7e-b47e-f66d909833fd";

		public static void Run() {
			if( string.IsNullOrEmpty(POST_URL) )
				return;
			AssetDatabase.Refresh();
			var logPath = $"{Application.dataPath}/../build.log";
			var fs = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			var sb = new StringBuilder();
			var errorExist = false;
			using( var sr = new StreamReader(fs) ) {
				while( !sr.EndOfStream ) {
					var s = sr.ReadLine();
					if( s != null ) {
						if( errorExist ) {
							if( s.Contains("CompilerOutput:-stderr") ) {
								errorExist = false;
							}
							else {
								sb.AppendLine(s);
							}
						}

						if( s.Contains("compilationhadfailure") ) {
							errorExist = true;
						}
					}
					else {
						if( sb.Length > 0 ) {
							var json = new JObject {
								{"msg_type", "text"}, {
									"content", new JObject {
										{"text", sb.ToString()}
									}
								}
							};

							var req = WebRequest.CreateHttp(POST_URL);
							req.Method = "POST";
							req.ContentType = "application/json";
							var stream = req.GetRequestStream();
							var buffer = Encoding.UTF8.GetBytes(json.ToString(Formatting.None));
							stream.Write(buffer, 0, buffer.Length);
							stream.Flush();

							var response = req.GetResponse();
							stream = response.GetResponseStream();
							var readByteCount = 0;
							buffer = new byte[response.ContentLength];
							while( readByteCount < response.ContentLength ) {
								readByteCount += stream.Read(buffer, readByteCount, buffer.Length - readByteCount);
							}

							var res = Encoding.UTF8.GetString(buffer);
							var jres = JObject.Parse(res);
							if( jres["errmsg"] != null && jres["errmsg"].Value<string>() != "ok" ) {
								Debug.LogError($"Response error : {res}");
							}
						}

						errorExist = false;
						sb.Clear();
					}
				}
			}

			EditorApplication.Exit(0);
		}
	}
}