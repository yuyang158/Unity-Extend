using System;
using System.Collections.Specialized;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Extend.Asset;
using Extend.Common;
using UnityEngine;

namespace Extend.DebugUtil {
	public class ErrorLogToFile : IService {
		public int ServiceType => (int) CSharpServiceManager.ServiceType.ERROR_LOG_TO_FILE;
		private TextWriter m_writer;
		private readonly AutoResetEvent m_autoEvent = new AutoResetEvent(false);
		private Thread m_writeThread;
		private Thread m_netStreamThread;
		private bool m_exit;
		private string m_errorLogPath;
		private string m_lastErrorLogPath;

		private readonly CancellationTokenSource m_cancelTokenSource = new CancellationTokenSource();
		private CancellationToken m_cancelToken;

		public void Initialize() {
			m_cancelToken = m_cancelTokenSource.Token;
#if UNITY_EDITOR
			m_errorLogPath = Application.persistentDataPath + "/error.log";
#elif UNITY_STANDALONE
			m_errorLogPath = "./error.log";
#else
			m_errorLogPath = Application.persistentDataPath + "/error.log";
#endif
			try {
				if( File.Exists(m_errorLogPath) ) {
#if UNITY_EDITOR
					m_lastErrorLogPath = Application.persistentDataPath + "/last-error.log";
#elif UNITY_STANDALONE
					m_lastErrorLogPath = "./last-error.log";
#else
					m_lastErrorLogPath = Application.persistentDataPath + "/last-error.log";
#endif
					if( File.Exists(m_lastErrorLogPath) ) {
						File.Delete(m_lastErrorLogPath);
					}

					File.Move(m_errorLogPath, m_lastErrorLogPath);
					File.Delete(m_errorLogPath);
				}
			}
			finally {
				var stream = new FileStream(m_errorLogPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
				m_writer = new StreamWriter(stream);
				Application.logMessageReceivedThreaded += HandleLogThreaded;
			}

			m_writeThread = new Thread(WriteThread);
			m_writeThread.Start();

			m_netStreamThread = new Thread(LogServerThread);
			m_netStreamThread.Start();
		}

		private void LogServerThread() {
			using UdpClient udpClient = new UdpClient(36677);
			while( true ) {
				try {
					var receiveTask = udpClient.ReceiveAsync();
					receiveTask.Wait(m_cancelToken);
					if( receiveTask.IsCanceled ) {
						break;
					}

					var cmd = Encoding.UTF8.GetString(receiveTask.Result.Buffer);
					string errorContent = "";
					if( cmd == "error.log" ) {
						lock( m_writer ) {
							m_writer.Close();
							errorContent = File.ReadAllText(m_errorLogPath);
							var stream = new FileStream(m_errorLogPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
							m_writer = new StreamWriter(stream);
						}
					}
					else if( cmd == "last-error.log" ) {
						errorContent = File.ReadAllText(m_lastErrorLogPath);
					}
					else if( cmd == "git-stat" ) {
						using var stream = FileLoader.LoadFileSync("BuildInfo.txt");
						using( var reader = new StreamReader(stream) ) {
							errorContent = reader.ReadToEnd();
						}
					}

					var buffer = Encoding.UTF8.GetBytes(errorContent);
					var lengthBuffer = BitConverter.GetBytes(buffer.Length);
					udpClient.Send(lengthBuffer, lengthBuffer.Length, receiveTask.Result.RemoteEndPoint);

					int sentCount = 0;
					var sendBuffer = new byte[512];
					while( buffer.Length - sentCount > 512 ) {
						Array.Copy(buffer, sentCount, sendBuffer, 0, 512);
						var sendTask = udpClient.SendAsync(sendBuffer, sendBuffer.Length, receiveTask.Result.RemoteEndPoint);
						sendTask.Wait(m_cancelToken);
						sentCount += 512;
					}

					Array.Copy(buffer, sentCount, sendBuffer, 0, buffer.Length - sentCount);
					udpClient.Send(sendBuffer, buffer.Length - sentCount, receiveTask.Result.RemoteEndPoint);
				}
				catch( SocketException ) {
				}
				catch( Exception ) {
					// Debug.LogWarning(e.Message);
					break;
				}
			}
		}

		private string m_log;
		private string m_stackTrace;

		private void WriteThread() {
			while( true ) {
				m_autoEvent.WaitOne();
				if( m_writer == null )
					return;
				lock( m_writer ) {
					if( m_exit ) {
						m_writer.Dispose();
						return;
					}

					m_writer.WriteLine(m_log);
					m_writer.Write(m_stackTrace);
					m_writer.Flush();
				}
			}
		}

		private void HandleLogThreaded(string message, string stackTrace, LogType type) {
			if( type is LogType.Assert or LogType.Error or LogType.Exception or LogType.Warning ) {
				var now = DateTime.Now;
				var typeStr = type.ToString().ToUpper();
				lock( m_writer ) {
					m_log = $"[{now.ToShortDateString()} {now.ToLongTimeString()}] [{typeStr}]: {message}";
					if( type is LogType.Assert or LogType.Exception or LogType.Error ) {
						m_stackTrace = stackTrace;
					}
					else {
						m_stackTrace = string.Empty;
					}
				}

				m_autoEvent.Set();
			}
		}

		public static void Upload(bool currentLog) {
			string filePath;
			if( currentLog ) {
				filePath = Application.persistentDataPath + "/error.log";
			}
			else {
				filePath = Application.persistentDataPath + "/last-error.log";
			}

			var releaseType = IniRead.SystemSetting.GetString("GAME", "Mode");
			var qs = new NameValueCollection {
				{"device", SystemInfo.deviceName},
				{"os", SystemInfo.operatingSystem},
				{"version", Application.version},
				{"other", releaseType},
				{"whoami", "bing"}
			};
			var url = IniRead.SystemSetting.GetString("DEBUG", "LogUploadUrl");
			Utility.HttpFileUpload(url, qs, filePath);
		}

		public void Destroy() {
			m_cancelTokenSource.Cancel();
			m_cancelTokenSource.Dispose();
			Application.logMessageReceivedThreaded -= HandleLogThreaded;
			m_exit = true;
			m_autoEvent.Set();
		}
	}
}
