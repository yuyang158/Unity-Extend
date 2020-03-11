using System;
using System.IO;
using Extend.Common;
using UnityEngine;

namespace Extend.DebugUtil {
	public class ErrorLogToFile : IService {
		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.ERROR_LOG_TO_FILE;

		private TextWriter writer;

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
				writer = new StreamWriter(new FileStream(errorLogPath, FileMode.CreateNew));
				Application.logMessageReceivedThreaded += HandleLogThreaded;
			}
		}

		private void HandleLogThreaded(string message, string stackTrace, LogType type) {
			if( type == LogType.Assert || type == LogType.Error || type == LogType.Exception || type == LogType.Warning ) {
				var now = DateTime.Now;
				var log = $"[{now.ToShortDateString()} {now.ToLongTimeString()}]: {message}";
				writer.WriteLineAsync(log);
			}
		}

		public void Destroy() {
			Application.logMessageReceivedThreaded -= HandleLogThreaded;
			writer.Close();
		}
	}
}