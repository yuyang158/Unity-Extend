using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Extend.Common.Editor {
	[InitializeOnLoad]
	public static class BuildErrorReport {
		private const string WEIXIN_POST_URL = "";
		static BuildErrorReport() {
			var t = new Thread(Run);
			t.Start();

			EditorApplication.quitting += () => { t.Abort(); };
		}

		private static void Run() {
			if(string.IsNullOrEmpty(WEIXIN_POST_URL))
				return;
			var wh = new AutoResetEvent(false);
#if UNITY_EDITOR_WIN
			var logPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/Unity/Editor/Editor.log";
#elif UNITY_EDITOR_OSX
			var logPath = "~/Library/Logs/Unity/Editor.log";
#else
			var logPath = "~/.config/unity3d/Editor.log";
#endif
			var dir = Path.GetDirectoryName(logPath);
			var fsw = new FileSystemWatcher(dir) {
				Filter = Path.GetFileName(logPath), EnableRaisingEvents = true
			};
			fsw.Changed += (s, e) => wh.Set();

			var fs = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			fs.Seek(0, SeekOrigin.End);
			var sb = new StringBuilder();
			var errorExist = false;
			using( var sr = new StreamReader(fs) ) {
				while( true ) {
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
								{"msgtype", "text"}, {
									"text", new JObject {
										{"content", sb.ToString()}
									}
								}
							};

							var req = WebRequest.CreateHttp(WEIXIN_POST_URL);
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
						wh.WaitOne();
					}
				}
			}
		}
	}
}