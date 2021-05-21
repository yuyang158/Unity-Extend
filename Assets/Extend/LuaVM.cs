using System;
using System.IO;
using System.Text;
using Extend.Asset;
using Extend.Common;
using Extend.LuaMVVM;
using Extend.LuaUtil;
using UnityEngine;
using XLua;
using Debug = UnityEngine.Debug;

namespace Extend {
	public class LuaVM : IService, IServiceUpdate, IDisposable {
		private LuaMemoryLeakChecker.Data leakData;
#if !UNITY_EDITOR
		private static readonly string LUA_DEBUG_DIRECTORY = Application.persistentDataPath + "/Lua/";
#endif
		private LuaFunction OnDestroy;
		private LuaFunction OnInit;
		private LuaEnv Default { get; set; }
		public LuaTable Global => Default.Global;
		public LuaTable DestroyedTableMeta { private set; get; }

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

		public int ServiceType => (int)CSharpServiceManager.ServiceType.LUA_SERVICE;
		private SetupLuaNewClassCallback m_newClassCallback;

		private void OnLuaNewClass(LuaTable classMeta, LuaTable parentClassMeta) {
			LuaClassCache.Register(classMeta, parentClassMeta);
		}

		public LuaClassCache LuaClassCache { get; private set; }
		public static Action OnVMCreated;
		public static Action OnVMQuiting;

		private static byte[] LoadFile(ref string filename, string extension) {
#if UNITY_EDITOR
			filename = filename.Replace('.', '/') + extension;
			var path = $"{Application.dataPath}/../Lua/{filename}";
			if( File.Exists(path) ) {
				var text = File.ReadAllText(path);
				return Encoding.UTF8.GetBytes(text);
			}

			return null;
#else
			filename = filename.Replace('.', '/') + extension;
			var hotfix = $"{LUA_DEBUG_DIRECTORY}{filename}";
			if( File.Exists(hotfix) ) {
				Debug.LogWarning("HOTFIX FILE : " + hotfix);
				filename += extension;
				return File.ReadAllBytes(hotfix);
			}
			
			var service = CSharpServiceManager.Get<AssetService>(CSharpServiceManager.ServiceType.ASSET_SERVICE);
			var assetRef = service.Load($"Lua/{filename}", typeof(TextAsset));
			if( assetRef.AssetStatus != AssetRefObject.AssetStatus.DONE )
				return null;
			return assetRef.GetTextAsset().bytes;
#endif
		}

		public void Initialize() {
			Default = new LuaEnv();
			LuaClassCache = new LuaClassCache();
			m_newClassCallback = OnLuaNewClass;
#if UNITY_EDITOR
			Default.AddLoader((ref string filename) => LoadFile(ref filename, ".lua"));
#else
			Default.AddLoader((ref string filename) => LoadFile(ref filename, ".lua"));
#endif
#if UNITY_EDITOR
			Default.SetProtoLoader((ref string filename) => LoadFile(ref filename, ".proto"));
#else
			Default.SetProtoLoader((ref string filename) => LoadFile(ref filename, ""));
#endif

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

			DestroyedTableMeta = Global.Get<LuaTable>("DestroyedTableMeta");
		}

		public void StartUp() {
			OnInit.Action<Func<string, byte[]>>(filename => LoadFile(ref filename, ".lua"));
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

		public static string LogCallStack() {
			var luaVm = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			var msg = luaVm.Default.Global.GetInPath<Func<string>>("debug.traceback");
			var str = "lua stack : " + msg.Invoke();
			Debug.Log(str);

			return str;
		}

		public void Destroy() {
			OnDestroy.Call();
			TempBindingExpressCache.Clear();
			OnVMQuiting?.Invoke();

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