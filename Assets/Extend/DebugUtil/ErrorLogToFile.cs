using System;
using System.IO;
using System.Threading;
using Extend.Common;
using UnityEngine;

namespace Extend.DebugUtil {
	public class ErrorLogToFile : IService {
		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.ERROR_LOG_TO_FILE;
		private TextWriter writer;
		private AutoResetEvent autoEvent = new AutoResetEvent(false);

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
				writer = new StreamWriter(errorLogPath);
				Application.logMessageReceivedThreaded += HandleLogThreaded;
			}

			var writeThread = new Thread(WriteThread);
			writeThread.Start();
		}

		private string log;
		private string _stackTrace;

		private void WriteThread() {
			while( true ) {
				autoEvent.WaitOne();
				lock( writer ) {
					writer.WriteLine(log);
					writer.Write(_stackTrace);
					writer.Flush();
				}
			}
		}

		private void HandleLogThreaded(string message, string stackTrace, LogType type) {
			if( type == LogType.Assert || type == LogType.Error || type == LogType.Exception || type == LogType.Warning ) {
				var now = DateTime.Now;
				var typeStr = type.ToString().ToUpper();
				lock( writer ) {
					log = $"[{now.ToShortDateString()} {now.ToLongTimeString()}] [{typeStr}]: {message}";
					if( type == LogType.Assert || type == LogType.Exception || type == LogType.Error ) {
						_stackTrace = stackTrace;
					}
					else {
						_stackTrace = string.Empty;
					}
				}
				autoEvent.Set();
			}
		}

		public void Destroy() {
			Application.logMessageReceivedThreaded -= HandleLogThreaded;
			writer.Close();
		}
	}
}