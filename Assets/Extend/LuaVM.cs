using System;
using System.IO;
using System.Text;
using Extend.Asset;
using Extend.Common;
using Extend.LuaUtil;
using UnityEngine;
using XLua;
using Debug = UnityEngine.Debug;
namespace Extend {
	public class LuaVM : IService, IServiceUpdate, IDisposable {
		private LuaMemoryLeakChecker.Data leakData;
		private static readonly string LUA_DEBUG_DIRECTORY = Application.persistentDataPath + "/Lua/";
		private LuaFunction OnDestroy;
		private LuaFunction OnInit;
		private LuaEnv Default { get; set; }
		public LuaTable Global => Default.Global;
		public static Action OnPreRequestLoaded;

		public LuaTable NewTable() {
			return Default.NewTable();
		}

		public long Memory => Default.Memory;
		public int LuaMapCount => Default.translator.objects.Count;

		public object[] LoadFileAtPath(string luaFileName) {
#if UNITY_EDITOR
			if( luaFileName.Contains("/") || luaFileName.Contains("\\") ) {
				Debug.LogError("Use . as a path separator : " + luaFileName);
				return null;
			}
#endif
			var ret = Default.DoString($"return require '{luaFileName}'");
			return ret;
		}

		public object[] DoString(string code, string chunkName = "chuck") {
			return Default.DoString(code, chunkName);
		}

		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.LUA_SERVICE;
		private SetupLuaNewClassCallback m_newClassCallback;

		private void OnLuaNewClass(LuaTable classMeta, LuaTable parentClassMeta) {
			LuaClassCache.Register(classMeta, parentClassMeta);
		}

		public LuaClassCache LuaClassCache { get; private set; }
		public static Action OnVMCreated;
		public static Action OnVMQuiting;

		public void Initialize() {
			Default = new LuaEnv();
			LuaClassCache = new LuaClassCache();
			m_newClassCallback = OnLuaNewClass;
			Default.AddLoader((ref string filename) => {
#if UNITY_EDITOR
				filename = filename.Replace('.', '/') + ".lua";
				var path = $"{Application.dataPath}/../Lua/{filename}";
				if( File.Exists(path) ) {
					var text = File.ReadAllText(path);
					return Encoding.UTF8.GetBytes(text);
				}

				return null;
#else
				filename = filename.Replace('.', '/') + ".lua";
				var hotfix = $"{LUA_DEBUG_DIRECTORY}{filename}.bytes";
				if( File.Exists(hotfix) ) {
					Debug.LogWarning("HOTFIX FILE : " + hotfix);
					filename += ".bytes";
					return File.ReadAllBytes(hotfix);
				}
				
				var service = CSharpServiceManager.Get<AssetService>(CSharpServiceManager.ServiceType.ASSET_SERVICE);
				var assetRef = service.Load($"Lua/{filename}", typeof(TextAsset));
				if( assetRef.AssetStatus != AssetRefObject.AssetStatus.DONE )
					return null;
				filename += ".bytes";
				return assetRef.GetTextAsset().bytes;
#endif
			});

			Default.SetProtoLoader((ref string filename) => {
#if UNITY_EDITOR
				filename = filename.Replace('.', '/') + ".proto";
				var path = $"{Application.dataPath}/../Lua/{filename}";
				if( File.Exists(path) ) {
					var text = File.ReadAllText(path);
					return Encoding.UTF8.GetBytes(text);
				}

				return null;
#else
				filename = filename.Replace('.', '/');
				var hotfix = $"{LUA_DEBUG_DIRECTORY}{filename}.proto";
				if( File.Exists(hotfix) ) {
					Debug.LogWarning("HOTFIX FILE : " + hotfix);
					filename += ".proto";
					return Encoding.UTF8.GetBytes(File.ReadAllText(hotfix));
				}
				
				var service = CSharpServiceManager.Get<AssetService>(CSharpServiceManager.ServiceType.ASSET_SERVICE);
				var assetRef = service.Load($"Lua/{filename}", typeof(TextAsset));
				if( assetRef.AssetStatus != AssetRefObject.AssetStatus.DONE )
					return null;
				filename += ".proto";
				return assetRef.GetTextAsset().bytes;
#endif
			});

			var setupNewCallback = LoadFileAtPath("base.class")[0] as LuaFunction;
			setupNewCallback.Action(m_newClassCallback);
			OnVMCreated?.Invoke();

			var ret = LoadFileAtPath("PreRequest")[0] as LuaTable;
			OnInit = ret.Get<LuaFunction>("init");
			OnDestroy = ret.Get<LuaFunction>("shutdown");
			
			OnPreRequestLoaded?.Invoke();
#if UNITY_EDITOR
			if( reportLeakMark )
				leakData = Default.StartMemoryLeakCheck();
#endif
			AssetService.Get().AddAfterDestroy(this);
		}

		public void StartUp() {
			OnInit.Call();
		}

		private GetGlobalVM m_getGlobalVMFunc;

		public object GetGlobalVM(string path) {
			if( m_getGlobalVMFunc == null ) {
				var getLuaService = Default.Global.GetInPath<GetLuaService>("_ServiceManager.GetService");
				var globalVMTable = getLuaService(5);
				m_getGlobalVMFunc = globalVMTable.GetInPath<GetGlobalVM>("GetVM");
			}

			return m_getGlobalVMFunc(path);
		}

		public LuaClassCache.LuaClass GetLuaClass(LuaTable klass) {
			return LuaClassCache.TryGetClass(klass);
		}
		
		public LuaClassCache.LuaClass GetLuaClass(string klass) {
			var ret = LoadFileAtPath(klass);
			if( !( ret[0] is LuaTable c ) )
				return null;
			return LuaClassCache.TryGetClass(c);
		}

		public void LogCallStack() {
			var msg = Default.Global.GetInPath<Func<string>>("debug.traceback");
			var str = "lua stack : " + msg.Invoke();
			Debug.Log(str);
		}

		public void Destroy() {
			OnVMQuiting?.Invoke();
			OnDestroy.Call();

			m_newClassCallback = null;
			OnInit = null;
			OnDestroy = null;
			OnVMCreated = null;
			OnPreRequestLoaded = null;
			OnVMQuiting = null;
			GC.Collect();
		}

		public void Update() {
			Default.Tick();
		}

#if UNITY_EDITOR
		private void ReportLeak() {
			var outputPath = Application.persistentDataPath + "/lua_memory_report.txt";
			using( var writer = new StreamWriter(outputPath) ) {
				writer.Write(Default.MemoryLeakReport(leakData, 2));
			}

			Debug.Log("Lua leak report : " + outputPath);
		}

		private const string PERF_KEY = "LUA_REPORT_LEAK";
		private const string MENU_NAME = "XLua/Report Lua Leak";
		private static bool reportLeakMark = UnityEditor.EditorPrefs.GetBool(PERF_KEY);

		[UnityEditor.MenuItem(MENU_NAME, true)]
		private static bool ValidateMemoryLeakReport() {
			UnityEditor.Menu.SetChecked(MENU_NAME, reportLeakMark);
			return true;
		}

		[UnityEditor.MenuItem("XLua/Report Memory Leak")]
		private static void IsMemoryLeakReport() {
			if( Application.isPlaying ) {
				UnityEditor.EditorUtility.DisplayDialog("ERROR", "Change report status before start play", "OK");
				return;
			}

			reportLeakMark = !reportLeakMark;
			UnityEditor.EditorPrefs.SetBool(PERF_KEY, reportLeakMark);
		}
#endif

		public void Dispose() {
#if UNITY_EDITOR
			if( reportLeakMark )
				ReportLeak();
#endif
			Default.Dispose();
		}
	}
}