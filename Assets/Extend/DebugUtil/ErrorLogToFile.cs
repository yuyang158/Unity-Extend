using System;
using System.IO;
using System.Threading;
using Extend.Common;
using UnityEngine;

namespace Extend.DebugUtil {
	public class ErrorLogToFile : IService {
		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.ERROR_LOG_TO_FILE;
		private TextWriter m_writer;
		private readonly AutoResetEvent m_autoEvent = new AutoResetEvent(false);
		private Thread m_writeThread;
		private bool m_exit;

		public void Initialize() {
			var errorLogPath = Application.persistentDataPath + "/error.log";
			try {
				if( File.Exists(errorLogPath) ) {
					var lastLogPath = Application.persistentDataPath + "/last-error.log";
					if( File.Exists(lastLogPath) ) {
						File.Delete(lastLogPath);
					}

					File.Move(errorLogPath, lastLogPath);
					File.Delete(errorLogPath);
				}
			}
			finally {
				m_writer = new StreamWriter(errorLogPath);
				Application.logMessageReceivedThreaded += HandleLogThreaded;
			}

			m_writeThread = new Thread(WriteThread);
			m_writeThread.Start();
		}

		private string log;
		private string _stackTrace;

		private void WriteThread() {
			while( true ) {
				m_autoEvent.WaitOne();
				if(m_writer == null)
					return;
				lock( m_writer ) {
					if( m_exit ) {
						m_writer.Close();
						break;
					}
					m_writer.WriteLine(log);
					m_writer.Write(_stackTrace);
					m_writer.Flush();
				}
			}
		}

		private void HandleLogThreaded(string message, string stackTrace, LogType type) {
			if( type == LogType.Assert || type == LogType.Error || type == LogType.Exception || type == LogType.Warning ) {
				var now = DateTime.Now;
				var typeStr = type.ToString().ToUpper();
				lock( m_writer ) {
					log = $"[{now.ToShortDateString()} {now.ToLongTimeString()}] [{typeStr}]: {message}";
					if( type == LogType.Assert || type == LogType.Exception || type == LogType.Error ) {
						_stackTrace = stackTrace;
					}
					else {
						_stackTrace = string.Empty;
					}
				}
				m_autoEvent.Set();
			}
		}

		public void Destroy() {
			Application.logMessageReceivedThreaded -= HandleLogThreaded;
			lock( m_writer ) {
				m_exit = true;
				m_autoEvent.Set();
			}
		}
	}
}