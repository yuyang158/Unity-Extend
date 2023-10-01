using System;
using System.Text;
using Extend.Common;
using TMPro;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;
using XLua;

namespace Extend.DebugUtil {
	[LuaCallCSharp, CSharpCallLua]
	public class InGameConsole : MonoBehaviour, IService {
		[SerializeField]
		private GameObject m_statGO;

		[SerializeField]
		private TextMeshProUGUI m_txtStat;

		[SerializeField]
		private TextMeshProUGUI m_txtError;

		[SerializeField]
		private GameObject m_errorRoot;

		public static Func<string> GlobalLuaLog;

		private LuaVM luaVM;
		private bool m_showFps;
		private bool m_showRender;
		private bool m_showMemory;
		private bool m_showStat;
		private bool m_showErrorPanel;

		private ProfilerRecorder m_drawCallCount;
		private ProfilerRecorder m_trianglesCount;
		private ProfilerRecorder m_verticesCount;

		private ProfilerRecorder m_totalAlloc;
		private ProfilerRecorder m_textureMemory;
		private ProfilerRecorder m_meshMemory;
		private ProfilerRecorder m_gcMemoryCount;
		private ProfilerRecorder m_frameAllocMemory;

		private void Awake() {
			luaVM = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);

			m_showFps = GameSystemSetting.Get().SystemSetting.GetBool("DEBUG", "ShowFPS");
			m_showRender = GameSystemSetting.Get().SystemSetting.GetBool("DEBUG", "ShowRender");
			m_showMemory = GameSystemSetting.Get().SystemSetting.GetBool("DEBUG", "ShowMemory");
			m_showStat = GameSystemSetting.Get().SystemSetting.GetBool("DEBUG", "ShowStat");
#if UNITY_EDITOR
			m_showErrorPanel = false;
#else
			m_showErrorPanel = GameSystemSetting.Get().SystemSetting.GetBool("DEBUG", "ShowErrorPanelWhenExceptionOrError");
#endif

			DontDestroyOnLoad(gameObject);
		}


		private void OnDisable() {
			m_drawCallCount.Dispose();
			m_trianglesCount.Dispose();
			m_verticesCount.Dispose();

			m_totalAlloc.Dispose();
			m_textureMemory.Dispose();
			m_meshMemory.Dispose();
			m_gcMemoryCount.Dispose();
			m_frameAllocMemory.Dispose();
		}

		private void OnEnable() {
			m_drawCallCount = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Total Batches Count");
			m_trianglesCount = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
			m_verticesCount = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Vertices Count");

			m_totalAlloc = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Reserved Memory");
			m_textureMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Texture Memory");
			m_meshMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Mesh Memory");
			m_gcMemoryCount = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Used Memory");
			m_frameAllocMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocation In Frame Count");
		}

		private readonly StringBuilder builder = new StringBuilder(4096);

		private void Update() {
			if( m_showErrorPanel ) {
				if( Input.touchCount >= 5 ) {
					m_errorRoot.SetActive(true);
				}
			}
			
			builder.Clear();
			if( m_showFps ) {
				builder.AppendFormat("FPS : {0} / {1}\n", Mathf.RoundToInt(1 / Time.smoothDeltaTime).ToString(),
					Application.targetFrameRate <= 0 ? "No Limit" : Application.targetFrameRate.ToString());
			}

			if( m_showRender ) {
				builder.AppendLine($"Draw Call : {m_drawCallCount.LastValue.ToString()}");
				builder.AppendLine($"Triangles Count : {m_trianglesCount.LastValue.ToString()}");
				builder.AppendLine($"Vertices Count : {m_verticesCount.LastValue.ToString()}");
			}

			if( m_showMemory ) {
				builder.AppendLine($"Mono : {( m_gcMemoryCount.LastValue / 1024 ).ToString()} KB");
				builder.AppendLine($"Lua : {luaVM.Memory.ToString()} KB");
				builder.AppendLine($"Unity : {( m_totalAlloc.LastValue / 1024 / 1024 ).ToString()} MB");
				builder.AppendLine($"Texture : {( m_textureMemory.LastValue / 1024 ).ToString()} KB");
				builder.AppendLine($"Mesh : {( m_meshMemory.LastValue / 1024 ).ToString()} KB");
				builder.AppendLine($"Alloc : {m_frameAllocMemory.LastValue.ToString()} Byte");
				if( Debug.isDebugBuild ) {
					var graphicsDriver = Profiler.GetAllocatedMemoryForGraphicsDriver() / 1024 / 1024;
					builder.AppendLine($"Graphics : {graphicsDriver.ToString()} MB");
				}
			}

			if( m_showStat ) {
				StatService.Get().Output(builder);
			}

			m_statGO.SetActive(builder.Length > 0);
			if( GlobalLuaLog != null ) {
				var log = GlobalLuaLog.Invoke();
				builder.AppendLine(log);
			}

			if( builder.Length > 0 )
				m_txtStat.text = builder.ToString();
		}

		public int ServiceType => (int) CSharpServiceManager.ServiceType.IN_GAME_CONSOLE;

		public void Initialize() {
			Application.logMessageReceived += ApplicationOnLogMessageReceivedThreaded;
		}

		private void ApplicationOnLogMessageReceivedThreaded(string condition, string stacktrace, LogType type) {
			if( type != LogType.Error && type != LogType.Exception ) {
				return;
			}

			if( m_showErrorPanel ) {
				m_errorRoot.SetActive(true);
			}

			m_txtError.text = $"{condition}\n{stacktrace}";
		}

		public void Destroy() {
			Application.logMessageReceived -= ApplicationOnLogMessageReceivedThreaded;
		}

		public void UploadLog() {
			ErrorLogToFile.Upload(true);
			Close();
		}

		public void UploadLogLast() {
			ErrorLogToFile.Upload(false);
			Close();
		}


		public void Close() {
			m_errorRoot.SetActive(false);
		}
	}
}
