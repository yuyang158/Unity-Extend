using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Extend.Common;
using Extend.LuaUtil;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace Extend.DebugUtil {
	public class InGameConsole : MonoBehaviour, IService {
		public KeyCode toggleKey = KeyCode.BackQuote;

		public int maxLogCount = 100;

		[SerializeField]
		private GameObject m_statGO;

		[SerializeField]
		private TextMeshProUGUI m_txtStat;

		[SerializeField]
		private TextMeshProUGUI m_txtLogTemplate;

		[SerializeField]
		private Button m_suggestTemplate;

		[SerializeField]
		private Transform m_logContentRoot;

		[SerializeField]
		private Transform m_suggestContentRoot;

		[SerializeField]
		private GameObject m_suggestScrollGO;

		[SerializeField]
		private ScrollRect m_logScroll;

		[SerializeField]
		private GameObject m_logGO;

		[SerializeField]
		private GameObject m_shortcut;

		[SerializeField]
		private TMP_InputField m_cmdInput;

		private static readonly Dictionary<LogType, Color> logTypeColors = new Dictionary<LogType, Color> {
			{LogType.Assert, Color.white},
			{LogType.Error, Color.red},
			{LogType.Exception, Color.red},
			{LogType.Log, Color.white},
			{LogType.Warning, Color.yellow},
		};

		private readonly Queue<Log> m_queuedLogs = new Queue<Log>();

		private LuaVM luaVM;
		private bool m_showConsoleWhenError;
		private bool m_showFps;
		private bool m_showMemory;
		private bool m_showStat;
		private bool m_logScrollVisible;

		private bool m_scrollToEnd = true;

		private bool LogScrollVisible {
			get => m_logScrollVisible;
			set {
				m_logScrollVisible = value;
				m_logGO.SetActive(value);
				m_statGO.SetActive(!value);
				m_shortcut.SetActive(value);
			}
		}

		private void Awake() {
			luaVM = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);

			m_showConsoleWhenError = GameSystem.Get().SystemSetting.GetBool("DEBUG", "ShowConsoleWhenError");
			m_showFps = GameSystem.Get().SystemSetting.GetBool("DEBUG", "ShowFPS");
			m_showMemory = GameSystem.Get().SystemSetting.GetBool("DEBUG", "ShowMemory");
			m_showStat = GameSystem.Get().SystemSetting.GetBool("DEBUG", "ShowStat");
			m_logScrollVisible = GameSystem.Get().SystemSetting.GetBool("DEBUG", "LogPanelGUIVisible");

			DontDestroyOnLoad(gameObject);
		}

		private class LuaCommandMatch : IComparable {
			public int Index;
			public LuaCommand Command;

			public int CompareTo(object obj) {
				var other = (LuaCommandMatch)obj;
				var ret = Index.CompareTo(other.Index);
				return ret == 0 ? string.Compare(Command.CommandName, other.Command.CommandName, StringComparison.Ordinal) : ret;
			}
		}

		public void OnLogScrollDrag() {
			m_scrollToEnd = m_logScroll.verticalNormalizedPosition < 0.0000001f;
		}

		private readonly Queue<string> m_cmdHistory = new Queue<string>();
		private int m_historyIndex = -1;

		private int HistoryIndex {
			get => m_historyIndex;
			set {
				m_historyIndex = value;
				if( m_historyIndex < 0 )
					return;
				if( m_historyIndex >= m_cmdHistory.Count )
					m_historyIndex = m_cmdHistory.Count - 1;
				var arr = m_cmdHistory.ToArray();
				m_cmdInput.text = arr[m_historyIndex];
			}
		}

		public void OnSendCommand() {
			if( string.IsNullOrWhiteSpace(m_cmdInput.text) )
				return;

			var input = m_cmdInput.text;
			var inputs = input.Split(' ');
			var cmd = inputs[0];
			string[] param = null;
			if( inputs.Length > 1 ) {
				param = inputs.Skip(1).Take(inputs.Length - 1).ToArray();
			}

			foreach( var luaCommand in luaCommands ) {
				if( luaCommand.CommandName.ToLower() == cmd.ToLower() ) {
					luaCommand.Command(param);
					break;
				}
			}

			while( m_cmdHistory.Count > 10 ) {
				m_cmdHistory.Dequeue();
			}

			m_cmdHistory.Enqueue(m_cmdInput.text);
			m_cmdInput.text = "";
			HistoryIndex = -1;
		}

		private readonly List<LuaCommandMatch> m_matches = new List<LuaCommandMatch>();

		public void OnInputCommandChanged() {
			foreach( Transform child in m_suggestContentRoot ) {
				Destroy(child.gameObject);
			}

			if( string.IsNullOrEmpty(m_cmdInput.text) ) {
				m_suggestScrollGO.SetActive(false);
				return;
			}

			m_matches.Clear();
			var parts = m_cmdInput.text.Split(' ');
			var inputCmd = parts[0];
			foreach( var luaCommand in luaCommands ) {
				var index = luaCommand.CommandName.IndexOf(inputCmd, StringComparison.CurrentCultureIgnoreCase);
				if( index >= 0 ) {
					m_matches.Add(new LuaCommandMatch() {
						Command = luaCommand,
						Index = index
					});
				}
			}

			m_matches.Sort();
			foreach( var match in m_matches ) {
				var luaCommand = match.Command;
				var suggestBtn = Instantiate(m_suggestTemplate, m_suggestContentRoot, false);
				suggestBtn.gameObject.SetActive(true);
				var txt = suggestBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
				txt.text = luaCommand.CommandName;
				suggestBtn.onClick.AddListener(() => { m_cmdInput.text = luaCommand.CommandName; });
			}

			m_suggestScrollGO.SetActive(m_matches.Count > 0);
		}

		private void HandleLogThreaded(string message, string stacktrace, LogType type) {
			Log log;
			if( m_queuedLogs.Count > maxLogCount ) {
				log = m_queuedLogs.Dequeue();
				log.text.color = logTypeColors[type];
				log.text.transform.SetAsLastSibling();
			}
			else {
				var text = Instantiate(m_txtLogTemplate, m_logContentRoot, false);
				text.gameObject.SetActive(true);
				text.color = logTypeColors[type];
				log = new Log() {
					text = text,
					type = type
				};
			}
			log.text.text = $"{message}\n{stacktrace}";
			m_queuedLogs.Enqueue(log);

			if( m_showConsoleWhenError && ( type == LogType.Assert || type == LogType.Error || type == LogType.Exception ) ) {
				LogScrollVisible = true;
			}

			if( m_scrollToEnd ) {
				m_logScroll.verticalNormalizedPosition = 0;
			}
		}

		private void OnDisable() {
			Application.logMessageReceivedThreaded -= HandleLogThreaded;
		}

		private void OnEnable() {
			Application.logMessageReceivedThreaded += HandleLogThreaded;
		}

		private readonly StringBuilder builder = new StringBuilder(1024);

		private void Update() {
			builder.Clear();
			if( m_showFps ) {
				builder.AppendFormat("FPS : {0} / {1}\n", Mathf.RoundToInt(1 / Time.smoothDeltaTime).ToString(),
					Application.targetFrameRate <= 0 ? "No Limit" : Application.targetFrameRate.ToString());
			}

			if( m_showMemory ) {
				var graphicsDriver = Profiler.GetAllocatedMemoryForGraphicsDriver() / 1024 / 1024;
				var unityTotalMemory = Profiler.GetTotalReservedMemoryLong() / 1024 / 1024;
				builder.AppendFormat("Mono : {0} KB\n", ( GC.GetTotalMemory(false) / 1024 ).ToString());
				builder.AppendFormat("Lua : {0} KB\n", luaVM.Memory.ToString());
				builder.AppendFormat("Lua Map : {0}\n", luaVM.LuaMapCount.ToString());
				builder.AppendFormat("Unity : {0} MB\n", unityTotalMemory.ToString());
				builder.AppendFormat("Texture : {0} KB\n", ( Texture.currentTextureMemory / 1024 ).ToString());
				if( Debug.isDebugBuild )
					builder.AppendFormat("Graphics : {0} MB\n", graphicsDriver.ToString());
			}

			if( m_showStat ) {
				StatService.Get().Output(builder);
			}

			m_statGO.SetActive(builder.Length > 0);
			if( builder.Length > 0 )
				m_txtStat.text = builder.ToString();

			if( Input.GetKeyDown(toggleKey) ) {
				LogScrollVisible = !LogScrollVisible;
			}

			if( !LogScrollVisible )
				return;

			if( Input.GetKeyDown(KeyCode.Tab) ) {
				if( m_matches.Count > 0 ) {
					m_cmdInput.text = m_matches.First().Command.CommandName;
				}
			}

			if( Input.GetKeyDown(KeyCode.Return) ) {
				OnSendCommand();
			}

			if( Input.GetKeyDown(KeyCode.UpArrow) ) {
				if( m_cmdHistory.Count == 0 )
					return;

				if( HistoryIndex < 0 ) {
					HistoryIndex = m_cmdHistory.Count - 1;
				}
				else if( HistoryIndex > 0 ) {
					HistoryIndex--;
				}
			}

			if( Input.GetKeyDown(KeyCode.DownArrow) ) {
				if( m_cmdHistory.Count == 0 )
					return;
				
				if(HistoryIndex < 0)
					return;
				HistoryIndex++;
			}
		}

		private class LuaCommand {
			public string CommandName;
			public LuaCommandDelegate Command;
		}

		private static readonly List<LuaCommand> luaCommands = new List<LuaCommand>();

		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.IN_GAME_CONSOLE;

		public void Initialize() {
			BuildLuaCommand();
		}

		private static void BuildLuaCommand() {
			luaCommands.Clear();
			var luaVm = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			var getServiceFunc = luaVm.Global.GetInPath<GetLuaService>("_ServiceManager.GetService");
			if( getServiceFunc == null ) {
				Debug.LogError("Get service return null");
				return;
			}
			var commands = getServiceFunc(3);
			if( commands == null ) {
				Debug.LogError("Get commands return null");
				return;
			}
			commands.ForEach((string cmdName, LuaCommandDelegate cmd) => {
				luaCommands.Add(new LuaCommand() {
					CommandName = cmdName,
					Command = cmd
				});
			});
		}

		[Button(ButtonSize.Medium)]
		public void RebuildLuaCommand() {
			if( !Application.isPlaying )
				return;
			BuildLuaCommand();
		}

		public void UploadLog(bool current) {
			ErrorLogToFile.Upload(current);
		}

		public void UploadStat() {
			StatService.Upload();
		}

		public void Destroy() {
		}
	}

	internal struct Log {
		public LogType type;
		public TextMeshProUGUI text;
	}
}