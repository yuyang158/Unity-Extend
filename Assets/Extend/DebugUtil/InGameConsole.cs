using System;
using System.Collections.Generic;
using System.Linq;
using Extend.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using XLua;

namespace Extend.DebugUtil {
	/// <summary>
	/// A console to display Unity's debug logs in-game.
	/// </summary>
	public class InGameConsole : MonoBehaviour, IService {
		#region Inspector Settings

		/// <summary>
		/// The hotkey to show and hide the console window.
		/// </summary>
		public KeyCode toggleKey = KeyCode.BackQuote;

		/// <summary>
		/// Whether to open as soon as the game starts.
		/// </summary>
		public bool openOnStart;

		/// <summary>
		/// Whether to only keep a certain number of logs, useful if memory usage is a concern.
		/// </summary>
		public bool restrictLogCount;

		/// <summary>
		/// Number of logs to keep before removing old ones.
		/// </summary>
		public int maxLogCount = 100;

		/// <summary>
		/// Font size to display log entries with.
		/// </summary>
		public int logFontSize = 16;

		#endregion

		private static readonly GUIContent clearLabel = new GUIContent("Clear", "Clear the contents of the console.");
		private static GUIStyle _windowStyle;
		private static readonly Color BackgroundColor = new Color(0.1f, 0.3f, 0.4f, 0.8f);

		private static GUIStyle windowStyle {
			get {
				if( _windowStyle == null ) {
					var backgroundTexture = new Texture2D(1, 1);
					backgroundTexture.SetPixel(0, 0, BackgroundColor);
					backgroundTexture.Apply();
					
					_windowStyle = new GUIStyle();
					_windowStyle.normal.background = backgroundTexture;
					_windowStyle.padding = new RectOffset(4, 4, 4, 4);
				}

				return _windowStyle;
			}
		}

		private static readonly Dictionary<LogType, Color> logTypeColors = new Dictionary<LogType, Color> {
			{LogType.Assert, Color.white},
			{LogType.Error, Color.red},
			{LogType.Exception, Color.red},
			{LogType.Log, Color.white},
			{LogType.Warning, Color.yellow},
		};

		private static EventSystem cachedEventSystem;

		private bool isCollapsed;
		private bool isVisible;
		private bool IsVisible {
			get => isVisible;
			set {
				isVisible = value;
				if( !cachedEventSystem ) {
					cachedEventSystem = EventSystem.current;
				}
				cachedEventSystem.enabled = !isVisible;
			}
		}
		private readonly List<Log> logs = new List<Log>();
		private readonly ConcurrentQueue<Log> queuedLogs = new ConcurrentQueue<Log>();

		private Vector2 scrollPosition;
		private Rect windowRect = new Rect(0, 0, Screen.width, Screen.height * 0.75f);

		private readonly Dictionary<LogType, bool> logTypeFilters = new Dictionary<LogType, bool> {
			{LogType.Assert, true},
			{LogType.Error, true},
			{LogType.Exception, true},
			{LogType.Log, true},
			{LogType.Warning, true},
		};

		#region MonoBehaviour Messages

		private void OnDisable() {
			Application.logMessageReceivedThreaded -= HandleLogThreaded;
		}

		private void OnEnable() {
			Application.logMessageReceivedThreaded += HandleLogThreaded;
		}

		private void OnGUI() {
			if( !IsVisible ) {
				return;
			}

			windowRect = GUILayout.Window(123456, windowRect, DrawWindow, string.Empty, windowStyle);
		}
		
		private class LuaCommand {
			public string CommandName;
			public LuaFunction Command;
		}
		
		private static readonly List<LuaCommand> luaCommands = new List<LuaCommand>();

		private void Start() {
			luaCommands.Clear();
			var luaVm = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			var getServiceFunc = luaVm.Default.Global.GetInPath<LuaFunction>("_ServiceManager.GetService");
			var commands = getServiceFunc.Call(3)[0] as LuaTable;
			commands.ForEach((string cmdName, LuaFunction cmd) => {
				luaCommands.Add(new LuaCommand() {
					CommandName = cmdName,
					Command = cmd
				});
			});
			
			if( openOnStart ) {
				IsVisible = true;
			}
		}

		private void Update() {
			UpdateQueuedLogs();

			if( Input.GetKeyDown(toggleKey) ) {
				IsVisible = !IsVisible;
			}
		}

		#endregion

		private static void DrawCollapsedLog(Log log, GUIStyle logStyle) {
			GUILayout.BeginHorizontal();

			GUILayout.Label(log.GetTruncatedMessage(), logStyle);
			GUILayout.FlexibleSpace();
			GUILayout.Label(log.count.ToString(), GUI.skin.box);

			GUILayout.EndHorizontal();
		}

		private static void DrawExpandedLog(Log log, GUIStyle logStyle) {
			for( var i = 0; i < log.count; i += 1 ) {
				GUILayout.Label(log.GetTruncatedMessage(), logStyle);
			}
		}

		private void DrawLog(Log log, GUIStyle logStyle) {
			GUI.contentColor = logTypeColors[log.type];

			if( isCollapsed ) {
				DrawCollapsedLog(log, logStyle);
			}
			else {
				DrawExpandedLog(log, logStyle);
			}
		}

		private void DrawLogList() {
			var badgeStyle = GUI.skin.box;
			badgeStyle.fontSize = logFontSize;

			var logStyle = GUI.skin.label;
			logStyle.fontSize = logFontSize;

			scrollPosition = GUILayout.BeginScrollView(scrollPosition);

			// Used to determine height of accumulated log labels.
			GUILayout.BeginVertical();

			var visibleLogs = logs.Where(IsLogVisible);

			foreach( var log in visibleLogs ) {
				DrawLog(log, logStyle);
			}

			GUILayout.EndVertical();
			var innerScrollRect = GUILayoutUtility.GetLastRect();
			GUILayout.EndScrollView();
			var outerScrollRect = GUILayoutUtility.GetLastRect();

			// If we're scrolled to bottom now, guarantee that it continues to be in next cycle.
			if( Event.current.type == EventType.Repaint && IsScrolledToBottom(innerScrollRect, outerScrollRect) ) {
				ScrollToBottom();
			}

			// Ensure GUI colour is reset before drawing other components.
			GUI.contentColor = Color.white;
		}

		private string command;
		private void DrawToolbar() {
			GUILayout.BeginHorizontal();

			var returnDown = Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return;
			command = GUILayout.TextField(command);
			if( !string.IsNullOrEmpty(command) ) {
				var readCommand = command.Split(' ')[0];
				var selecteds = luaCommands.FindAll(luaCommand => luaCommand.CommandName.StartsWith(readCommand));
				if( selecteds.Count > 0 ) {
					var rect = GUILayoutUtility.GetLastRect();
					var singleLineHeight = rect.height + 2;
					rect.y -= singleLineHeight * selecteds.Count;
					rect.height = singleLineHeight * selecteds.Count;
					GUI.Box(rect, new GUIContent(Texture2D.blackTexture));
					rect.x += 5;
					rect.height = singleLineHeight;
					foreach( var luaCommand in selecteds ) {
						GUI.Label(rect, luaCommand.CommandName);
						rect.y += singleLineHeight;
					}
				}
			}
			
			GUILayout.Space(5);
			if( (GUILayout.Button("OK", GUILayout.Width(80)) || returnDown) && !string.IsNullOrEmpty(command) ) {
				var split = command.Split(' ');
				var cmd = luaCommands.Find(luaCommand => luaCommand.CommandName == split[0]);
				if( cmd != null ) {
					var param = new object[split.Length - 1];
					if( split.Length > 1 ) {
						Array.Copy(split, 1, param, 0, split.Length - 1);
					}
					cmd.Command.Call(param);
					command = "";
				}
			}
			GUILayout.Space(5);

			if( GUILayout.Button(clearLabel, GUILayout.Width(80)) ) {
				logs.Clear();
			}

			foreach( LogType logType in Enum.GetValues(typeof(LogType)) ) {
				var currentState = logTypeFilters[logType];
				var label = logType.ToString();
				logTypeFilters[logType] = GUILayout.Toggle(currentState, label, GUILayout.ExpandWidth(false));
				GUILayout.Space(20);
			}

			GUILayout.EndHorizontal();
		}

		private void DrawWindow(int windowID) {
			DrawLogList();
			DrawToolbar();
		}

		private Log? GetLastLog() {
			if( logs.Count == 0 ) {
				return null;
			}

			return logs.Last();
		}

		private void UpdateQueuedLogs() {
			while( queuedLogs.TryDequeue(out var log) ) {
				ProcessLogItem(log);
			}
		}

		private void HandleLogThreaded(string message, string stackTrace, LogType type) {
			var now = DateTime.Now;
			var log = new Log {
				count = 1,
				message = $"[{now.ToShortDateString()} {now.ToLongTimeString()}]: {message}",
				type = type,
			};

			// Queue the log into a ConcurrentQueue to be processed later in the Unity main thread,
			// so that we don't get GUI-related errors for logs coming from other threads
			queuedLogs.Enqueue(log);
		}

		private void ProcessLogItem(Log log) {
			var lastLog = GetLastLog();
			var isDuplicateOfLastLog = lastLog.HasValue && log.Equals(lastLog.Value);

			if( isDuplicateOfLastLog ) {
				// Replace previous log with incremented count instead of adding a new one.
				log.count = lastLog.Value.count + 1;
				logs[logs.Count - 1] = log;
			}
			else {
				logs.Add(log);
				TrimExcessLogs();
			}
		}

		private bool IsLogVisible(Log log) {
			return logTypeFilters[log.type];
		}

		private bool IsScrolledToBottom(Rect innerScrollRect, Rect outerScrollRect) {
			var innerScrollHeight = innerScrollRect.height;

			// Take into account extra padding added to the scroll container.
			var outerScrollHeight = outerScrollRect.height - GUI.skin.box.padding.vertical;

			// If contents of scroll view haven't exceeded outer container, treat it as scrolled to bottom.
			// Scrolled to bottom (with error margin for float math)
			return outerScrollHeight > innerScrollHeight || Mathf.Approximately(innerScrollHeight, scrollPosition.y + outerScrollHeight);
		}

		private void ScrollToBottom() {
			scrollPosition = new Vector2(0, float.MaxValue);
		}

		private void TrimExcessLogs() {
			if( !restrictLogCount ) {
				return;
			}

			var amountToRemove = logs.Count - maxLogCount;
			if( amountToRemove <= 0 ) {
				return;
			}

			logs.RemoveRange(0, amountToRemove);
		}

		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.IN_GAME_CONSOLE;
		public void Initialize() {
			
		}

		public void Destroy() {
			Destroy(this);
		}
	}

	/// <summary>
	/// A basic container for log details.
	/// </summary>
	internal struct Log {
		public int count;
		public string message;
		// public string stackTrace;
		public LogType type;

		/// <summary>
		/// The max string length supported by UnityEngine.GUILayout.Label without triggering this error:
		/// "String too long for TextMeshGenerator. Cutting off characters."
		/// </summary>
		private const int maxMessageLength = 16382;

		public bool Equals(Log log) {
			return message == log.message && type == log.type;
		}

		/// <summary>
		/// Return a truncated message if it exceeds the max message length.
		/// </summary>
		public string GetTruncatedMessage() {
			if( string.IsNullOrEmpty(message) ) {
				return message;
			}

			return message.Length <= maxMessageLength ? message : message.Substring(0, maxMessageLength);
		}
	}

	/// <summary>
	/// Alternative to System.Collections.Concurrent.ConcurrentQueue
	/// (It's only available in .NET 4.0 and greater)
	/// </summary>
	/// <remarks>
	/// It's a bit slow (as it uses locks), and only provides a small subset of the interface
	/// Overall, the implementation is intended to be simple & robust
	/// </remarks>
	internal class ConcurrentQueue<T> {
		readonly Queue<T> queue = new Queue<T>();
		readonly object queueLock = new object();

		public void Enqueue(T item) {
			lock( queueLock ) {
				queue.Enqueue(item);
			}
		}

		public bool TryDequeue(out T result) {
			lock( queueLock ) {
				if( queue.Count == 0 ) {
					result = default(T);
					return false;
				}

				result = queue.Dequeue();
				return true;
			}
		}
	}
}