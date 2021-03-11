using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace Extend.Common.Editor {
#if NOTIFICATION_WHEN_COMPILE_ERROR
	[InitializeOnLoad]
#endif
	public static class BuildErrorReport {
		private const string POST_URL = "https://open.feishu.cn/open-apis/bot/v2/hook/7edd60ad-1e5e-4c7e-b47e-f66d909833fd";

#if NOTIFICATION_WHEN_COMPILE_ERROR
		static BuildErrorReport() {
			var t = new Thread(Run);
			t.Start();
			SVNUpdate();
		}
#endif

		private static HttpListener m_listener;
		public static string m_url = "http://pw-win.private-tunnel.site:6081/";

		private static async void SVNUpdate() {
			var paths = Environment.GetEnvironmentVariable("Path");
			var pathArray = paths.Split(';');
			string svnDir = string.Empty;
			foreach( var path in pathArray ) {
				if( path.Contains("SVN") ) {
					Debug.Log(path);
					svnDir = path;
					break;
				}
			}

			m_listener = new HttpListener();
			m_listener.Prefixes.Add(m_url);
			try {
				m_listener.Start();
			}
			catch( Exception e ) {
				Debug.LogException(e);
				return;
			}

			while( true ) {
				var ctx = await m_listener.GetContextAsync();
				var req = ctx.Request;
				var resp = ctx.Response;
				
				Debug.LogWarning($"Request : {DateTime.Now}");

				if( req.Url.AbsolutePath == "/shutdown" ) {
					break;
				}

				byte[] data = Encoding.UTF8.GetBytes($"Sync at {DateTime.Now}");
				resp.ContentType = "text/html";
				resp.ContentEncoding = Encoding.UTF8;
				resp.ContentLength64 = data.LongLength;

				await resp.OutputStream.WriteAsync(data, 0, data.Length);
				resp.Close();

				Debug.Log("SVN Update Start");
				var process = new Process {
					StartInfo = {
						RedirectStandardOutput = true,
						UseShellExecute = true,
						FileName = $"{svnDir}/svn.exe",
						Arguments = "update"
					}
				};
				process.Start();
				process.WaitForExit();
				Debug.Log("SVN Update Finished");
				AssetDatabase.Refresh();
				Debug.Log("Asset Database Refreshed...");
			}

			EditorApplication.Exit(0);
		}

		private static void Run() {
			if( string.IsNullOrEmpty(POST_URL) )
				return;
			var wh = new AutoResetEvent(false);
#if UNITY_EDITOR_WIN
			var logPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/Unity/Editor/Editor.log";
#elif UNITY_EDITOR_OSX
			var logPath = "~/Library/Logs/Unity/Editor.log";
#else
			var logPath = "~/.config/unity3d/Editor.log";
#endif
			Debug.Log($"Watch for : {logPath}");
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
						wh.WaitOne();
					}
				}
			}
		}
	}
}