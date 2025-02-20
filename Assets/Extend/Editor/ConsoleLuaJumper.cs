using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Extend.Editor {
	public static class ConsoleLuaJumper {
		[OnOpenAssetAttribute(0)]
		private static bool OnOpenAsset(int instanceID, int line) {
			var stackTrace = GetStackTrace();
			if( !string.IsNullOrEmpty(stackTrace) ) {
				string traceFirstLine = GetTraceFirstLine(stackTrace);
				ParseFilepathAndLineInFirstLine(traceFirstLine, out var filePath, out var lineStr);
				if( string.IsNullOrEmpty(filePath) ) {
					return false;
				}

				string appPath = EditorPrefs.GetString("kScriptsDefaultApp_h2657262712");
				// ide 需要事先设置命令好绑定
				if( appPath.Contains("Rider") ) {
					System.Diagnostics.Process.Start($"\"{appPath}\"", $"--line {lineStr} {filePath}");
					return true;
				}

				if( appPath.Contains("Code") ) {
					string binpath = appPath.Replace("Code.exe", "bin/code");
					System.Diagnostics.Process.Start($"\"{binpath}\"", $"-r -g \"{filePath}:{lineStr}\"");
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// 反射出日志堆栈
		/// </summary>
		/// <returns></returns>
		private static string GetStackTrace() {
			var consoleWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow");
			var fieldInfo = consoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
			var consoleWindowInstance = fieldInfo.GetValue(null);

			if( null != consoleWindowInstance ) {
				if( EditorWindow.focusedWindow.Equals(consoleWindowInstance) ) {
					fieldInfo = consoleWindowType.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
					string activeText = fieldInfo.GetValue(consoleWindowInstance).ToString();
					return activeText;
				}
			}

			return "";
		}

		private static string GetTraceFirstLine(string allContent) {
			string[] lines = allContent.Split('\n');
			foreach( var t in lines ) {
				if( t.Contains(".lua:") ) {
					return t;
				}
			}

			return string.Empty;
		}

		private static void ParseFilepathAndLineInFirstLine(string firstLine, out string filepath, out string line) {
			string[] parts = firstLine.Split(':', StringSplitOptions.RemoveEmptyEntries);
			for( int i = 0; i < parts.Length; i++ ) {
				if( int.TryParse(parts[i], out _) ) {
					line = parts[i];
					filepath = "Lua/" + parts[i - 1].Trim();
					return;
				}
			}

			filepath = string.Empty;
			line = string.Empty;
		}
	}
}