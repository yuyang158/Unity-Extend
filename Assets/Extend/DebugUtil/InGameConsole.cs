using System;
using System.Collections.Generic;
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
		private Transform m_logContentRoot;
		
		[SerializeField]
		private ScrollRect m_logScroll;

		[SerializeField]
		private GameObject m_logGO;

		[SerializeField]
		private InputField m_cmdInput;

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

		public void OnLogScrollDrag() {
			m_scrollToEnd = m_logScroll.verticalNormalizedPosition < 0.0000001f;
		}

		public void OnInputCommandChanged() {

		}

		private void HandleLogThreaded(string message, string stacktrace, LogType type) {
			Log log;
			if( m_queuedLogs.Count > maxLogCount ) {
				log = m_queuedLogs.Dequeue();
				log.text.text = message;
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
			m_queuedLogs.Enqueue(log);

			if( m_showConsoleWhenError && (type == LogType.Assert || type == LogType.Error || type == LogType.Exception) ) {
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

			if( builder.Length > 0 )
				m_txtStat.text = builder.ToString();

			if( Input.GetKeyDown(toggleKey) ) {
				LogScrollVisible = !LogScrollVisible;
			}

			if( Input.GetKeyDown(KeyCode.Tab) ) {
				
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
			var commands = getServiceFunc(3);
			commands.ForEach((string cmdName, LuaCommandDelegate cmd) => {
				luaCommands.Add(new LuaCommand() {
					CommandName = cmdName,
					Command = cmd
				});
			});
		}

		[Button(ButtonSize.Medium)]
		public void RebuildLuaCommand() {
			if(!Application.isPlaying)
				return;
			BuildLuaCommand();
		}

		public void Destroy() {
		}
	}

	internal struct Log {
		public LogType type;
		public TextMeshProUGUI text;
	}
}