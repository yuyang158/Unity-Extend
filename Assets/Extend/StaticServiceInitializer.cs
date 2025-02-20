#if !UNITY_EDITOR
using System.Text;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using XLua;
#endif
using System.Reflection;
using DG.Tweening;
using Extend.Asset;
using Extend.Common;
using Extend.DebugUtil;
using Extend.LuaUtil;
using Extend.Network;
using Extend.Network.HttpClient;
using Extend.SceneManagement;
using Extend.UI.i18n;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Extend {
#if UNITY_EDITOR
	[UnityEditor.InitializeOnLoad]
#endif
	internal static class StaticServiceInitializer {
#if UNITY_EDITOR
		static StaticServiceInitializer() {
			TMP_Settings settings =
				UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_Settings>("Assets/TextMesh Pro/Res/TMP Settings.asset");
			var settingsType = settings.GetType();
			var settingsInstanceInfo =
				settingsType.GetField("s_Instance", BindingFlags.Static | BindingFlags.NonPublic);
			settingsInstanceInfo.SetValue(null, settings);
		}
#endif

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		private static void OnInitAssembliesLoaded() {
			CSharpServiceManager.Initialize();
			CSharpServiceManager.Register(new ErrorLogToFile());
			CSharpServiceManager.Register(new StatService());
			CSharpServiceManager.Register(new AssetService());
			CSharpServiceManager.Register(new GameSystemSetting());
#if CLOSE_UNITY_LOG
				Debug.unityLogger.logEnabled = false;
#endif
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void OnInitBeforeSceneLoad() {
			// Application.runInBackground = true;
			var reference = AssetService.Get().Load<TMP_Settings>("Assets/TextMesh Pro/Res/TMP Settings.asset");
			TMP_Settings settings = reference.GetScriptableObject<TMP_Settings>();
			var settingsType = settings.GetType();
			var settingsInstanceInfo =
				settingsType.GetField("s_Instance", BindingFlags.Static | BindingFlags.NonPublic);
			settingsInstanceInfo.SetValue(null, settings);
			DOTween.Init(false, true, LogBehaviour.Default);
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		public static void OnSceneLoaded() {
			CSharpServiceManager.InitializeServiceGameObject();
			CSharpServiceManager.Register(new NetworkService());
			CSharpServiceManager.Register(new GlobalCoroutineRunnerService());
			CSharpServiceManager.Register(new LuaVM());
			CSharpServiceManager.Register(new TickService());
			CSharpServiceManager.Register(new I18nService(ConfigUtil.LoadConfigToJson("StaticText")));
			CSharpServiceManager.Register(new SceneLoadManager());
			var lua = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
#if DEMO_VERSION
			lua.Global.SetInPath("__DEMO_VERSION__", true);
#else
			lua.Global.SetInPath("__DEMO_VERSION__", false);
#endif

			HttpFileRequest.CacheFileExpireCheck();
			var scene = SceneManager.GetActiveScene();
			if( scene.name == "StartUp" ) {
				lua.StartUp(null);
			}
			else if( scene.name == "AbilityScene" ) {
				lua.StartUp("Game.State.AbilityTestState");
			}
			else {
				lua.StartUp("Game.State.DummyState");
			}
#if !DISABLESTEAMWORKS
			var instance = Platforms.Steam.SteamManager.Instance;
#endif
			Application.targetFrameRate = -1;
		}
	}
}