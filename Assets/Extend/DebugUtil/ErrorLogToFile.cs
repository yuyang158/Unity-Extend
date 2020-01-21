﻿using System;
using System.IO;
using Extend.Common;
using UnityEngine;

namespace Extend.DebugUtil {
	public class ErrorLogToFile : IService {
		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.ERROR_LOG_TO_FILE;

		private TextWriter writer;
		public void Initialize() {
			Application.logMessageReceivedThreaded += HandleLogThreaded;
			writer = new StreamWriter(new FileStream(Application.persistentDataPath + "/error.log", FileMode.Append));
		}

		private void HandleLogThreaded(string message, string stackTrace, LogType type) {
			if( type == LogType.Assert || type == LogType.Error || type == LogType.Exception || type == LogType.Warning ) {
				var now = DateTime.Now;
				var log = $"[{now.ToLongTimeString()}]: {message}";
				writer.WriteLineAsync(log);
			}
		}

		public void Destroy() {
			writer.Close();
			Application.logMessageReceivedThreaded -= HandleLogThreaded;
		}
	}
}