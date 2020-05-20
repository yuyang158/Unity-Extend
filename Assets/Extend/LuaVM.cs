using System;
using System.IO;
using Extend.Asset;
using Extend.Common;
using UnityEngine;
using XLua;
using Debug = UnityEngine.Debug;

namespace Extend {
	public class LuaVM : IService, IServiceUpdate, IDisposable {
		private LuaMemoryLeakChecker.Data leakData;
		private static readonly string LUA_DEBUG_DIRECTORY = Application.persistentDataPath + "/Lua/";
		private LuaFunction OnDestroy;
		private LuaEnv Default { get; set; }
		public LuaTable Global => Default.Global;

		public LuaTable NewTable() {
			return Default.NewTable();
		}

		public long Memory => Default.Memory;

		public object[] LoadFileAtPath(string luaFileName) {
			luaFileName = luaFileName.Replace('/', '.');
			var ret = Default.DoString($"return require '{luaFileName}'");
			return ret;
		}

		public object[] DoString(string code, string chunkName = "chuck") {
			return Default.DoString(code, chunkName);
		}

		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.LUA_SERVICE;

		public void Initialize() {
			Default = new LuaEnv();
			Default.AddLoader((ref string filename) => {
				filename = filename.Replace('.', '/');
				var hotfix = $"{LUA_DEBUG_DIRECTORY}{filename}.lua";
				if( File.Exists(hotfix) ) {
					filename += ".lua";
					return File.ReadAllBytes(hotfix);
				}
				
				var service = CSharpServiceManager.Get<AssetService>(CSharpServiceManager.ServiceType.ASSET_SERVICE);
				var assetRef = service.Load($"Lua/{filename}", typeof(TextAsset));
				if( assetRef.AssetStatus != AssetRefObject.AssetStatus.DONE )
					return null;
				filename += ".lua";
				return assetRef.GetTextAsset().bytes;
			});

			LoadFileAtPath("class");
			OnDestroy = LoadFileAtPath("PreRequest")[0] as LuaFunction;
#if UNITY_EDITOR
			if( reportLeakMark )
				leakData = Default.StartMemoryLeakCheck();
#endif
		}

		public void LogCallStack() {
			var msg = Default.Global.GetInPath<Func<string>>("debug.traceback");
			var str = "lua stack : " + msg;
			Debug.Log(str);
		}

		public void Destroy() {
			OnDestroy.Call();
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
		}
	}
}