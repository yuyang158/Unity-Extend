using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Extend.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;
using XLua;

namespace Extend.DebugUtil {
	[CSharpCallLua]
	public delegate LuaTable GetLuaService(int index);

	public class InGameConsole : MonoBehaviour, IService {
		public KeyCode toggleKey = KeyCode.BackQuote;

		public bool openOnStart;

		public bool restrictLogCount;

		public int maxLogCount = 100;

		public int logFontSize = 16;

		private static readonly GUIContent CLEAR_LABEL = new GUIContent("Clear", "Clear the contents of the console.");
		private static readonly Color BACKGROUND_COLOR = new Color(0.1f, 0.3f, 0.4f, 0.8f);

		private static GUIStyle m_windowStyle;
		private static GUIStyle windowStyle {
			get {
				if( m_windowStyle == null ) {
					var backgroundTexture = new Texture2D(1, 1);
					backgroundTexture.SetPixel(0, 0, BACKGROUND_COLOR);
					backgroundTexture.Apply();

					m_windowStyle = new GUIStyle {
						normal = {background = backgroundTexture},
						padding = new RectOffset(4, 4, 4, 4)
					};
				}

				return m_windowStyle;
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

		private bool m_isCollapsed;
		private bool m_isVisible;
		private bool IsVisible {
			get => m_isVisible;
			set {
				m_isVisible = value;
				if( !cachedEventSystem ) {
					cachedEventSystem = EventSystem.current;
				}
				cachedEventSystem.enabled = !m_isVisible;
			}
		}
		private readonly List<Log> m_logs = new List<Log>();
		private readonly ConcurrentQueue<Log> m_queuedLogs = new ConcurrentQueue<Log>();

		private Vector2 m_scrollPosition;
		private Rect m_windowRect = new Rect(0, 0, Screen.width, Screen.height * 0.75f);

		private readonly Dictionary<LogType, bool> logTypeFilters = new Dictionary<LogType, bool> {
			{LogType.Assert, true},
			{LogType.Error, true},
			{LogType.Exception, true},
			{LogType.Log, true},
			{LogType.Warning, true},
		};

		#region MonoBehaviour Messages

		private LuaVM luvVM;
		private void Awake() {
			luvVM = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			logFontSize = (int)(16 * Screen.dpi / 96.0f);
			if( logFontSize > 35 ) {
				logFontSize = 35;
			}
		}

		private void OnDisable() {
			Application.logMessageReceivedThreaded -= HandleLogThreaded;
		}

		private void OnEnable() {
			Application.logMessageReceivedThreaded += HandleLogThreaded;
		}

		private readonly StringBuilder builder = new StringBuilder(128);

		private GUIStyle style;
		private GUIStyle Style {
			get {
				if( style == null ) {
					style = new GUIStyle(GUI.skin.box) {
						alignment = TextAnchor.MiddleLeft,
						fontSize = logFontSize
					};
				}

				return style;
			}
		}
		private void OnGUI() {
			if( !IsVisible ) {
				builder.Clear();
				StatService.Get().Output(builder);
				
				builder.AppendFormat("FPS : {0} / {1}\n", Mathf.RoundToInt(1 / Time.smoothDeltaTime), 
					Application.targetFrameRate <= 0 ? "No Limit" : Application.targetFrameRate.ToString());
				var graphicsDriver = Profiler.GetAllocatedMemoryForGraphicsDriver() / 1024 / 1024;
				var unityTotalMemory = Profiler.GetTotalReservedMemoryLong() / 1024 / 1024;
				builder.AppendFormat("Mono : {0} KB\n", GC.GetTotalMemory(false) / 1024);
				builder.AppendFormat("Lua : {0} KB\n", luvVM.Memory);
				builder.AppendFormat("Unity : {0} MB\n", unityTotalMemory);
				builder.AppendFormat("Texture : {0} KB\n", Texture.currentTextureMemory / 1024);
				if(Debug.isDebugBuild)
					builder.AppendFormat("Graphics : {0} MB", graphicsDriver);
				GUILayout.Box(builder.ToString(), Style);
				return;
			}

			m_windowRect = GUILayout.Window(123456, m_windowRect, DrawWindow, string.Empty, windowStyle);
		}
		
		private class LuaCommand {
			public string CommandName;
			public LuaCommandDelegate Command;
		}
		
		private static readonly List<LuaCommand> luaCommands = new List<LuaCommand>();

		private void Start() {
			luaCommands.Clear();
			var luaVm = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			var getServiceFunc = luaVm.Global.GetInPath<GetLuaService>("_ServiceManager.GetService");
			var commands = getServiceFunc(3);
			commands.ForEach((string cmdName, LuaCommandDelegate cmd) => {
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

			if( m_isCollapsed ) {
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

			m_scrollPosition = GUILayout.BeginScrollView(m_scrollPosition);

			// Used to determine height of accumulated log labels.
			GUILayout.BeginVertical();

			var visibleLogs = m_logs.Where(IsLogVisible);

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
					rect.x += 5;
					rect.height = singleLineHeight;
					foreach( var luaCommand in selecteds ) {
						if( GUI.Button(rect, luaCommand.CommandName) ) {
							command = luaCommand.CommandName;
						}
						rect.y += singleLineHeight;
					}

					if( Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Tab ) {
						command = selecteds[0].CommandName;
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
					cmd.Command(param);
					command = "";
				}
			}
			GUILayout.Space(5);

			if( GUILayout.Button(CLEAR_LABEL, GUILayout.Width(80)) ) {
				m_logs.Clear();
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
			if( m_logs.Count == 0 ) {
				return null;
			}

			return m_logs.Last();
		}

		private void UpdateQueuedLogs() {
			while( m_queuedLogs.TryDequeue(out var log) ) {
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
			m_queuedLogs.Enqueue(log);
		}

		private void ProcessLogItem(Log log) {
			var lastLog = GetLastLog();
			var isDuplicateOfLastLog = lastLog.HasValue && log.Equals(lastLog.Value);

			if( isDuplicateOfLastLog ) {
				// Replace previous log with incremented count instead of adding a new one.
				log.count = lastLog.Value.count + 1;
				m_logs[m_logs.Count - 1] = log;
			}
			else {
				m_logs.Add(log);
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
			return outerScrollHeight > innerScrollHeight || Mathf.Approximately(innerScrollHeight, m_scrollPosition.y + outerScrollHeight);
		}

		private void ScrollToBottom() {
			m_scrollPosition = new Vector2(0, float.MaxValue);
		}

		private void TrimExcessLogs() {
			if( !restrictLogCount ) {
				return;
			}

			var amountToRemove = m_logs.Count - maxLogCount;
			if( amountToRemove <= 0 ) {
				return;
			}

			m_logs.RemoveRange(0, amountToRemove);
		}

		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.IN_GAME_CONSOLE;
		public void Initialize() {
			
		}

		public void Destroy() {
			Destroy(this);
		}
	}

	internal struct Log {
		public int count;
		public string message;
		// public string stackTrace;
		public LogType type;

		private const int maxMessageLength = 16382;

		public bool Equals(Log log) {
			return message == log.message && type == log.type;
		}

		public string GetTruncatedMessage() {
			if( string.IsNullOrEmpty(message) ) {
				return message;
			}

			return message.Length <= maxMessageLength ? message : message.Substring(0, maxMessageLength);
		}
	}

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