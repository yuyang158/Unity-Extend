using System;
using System.IO;
using Extend.AssetService;
using Extend.Common;
using UnityEngine;
using XLua;
using Debug = UnityEngine.Debug;

namespace Extend {
	public class LuaVM : IService, IServiceUpdate, IDisposable {
		private LuaMemoryLeakChecker.Data leakData;
		private LuaFunction OnDestroy;
		public LuaEnv Default { get; private set; }

		public object[] LoadFileAtPath(string luaFileName) {
			luaFileName = luaFileName.Replace('/', '.');
			var ret = Default.DoString($"return require '{luaFileName}'");
			return ret;
		}

		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.LUA_SERVICE;

		public void Initialize() {
			Default = new LuaEnv();
			Default.AddLoader((ref string filename) => {
				filename = filename.Replace('.', '/');
				var service = CSharpServiceManager.Get<AssetService.AssetService>(CSharpServiceManager.ServiceType.ASSET_SERVICE);
				var assetRef = service.Load($"Lua/{filename}", typeof(TextAsset));
				if( assetRef == null || assetRef.AssetStatus != AssetRefObject.AssetStatus.DONE )
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

		public void Destroy() {
			OnDestroy.Call();
			OnDestroy.Dispose();
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