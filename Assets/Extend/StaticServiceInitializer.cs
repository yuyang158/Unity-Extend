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
using Extend.Network;
using TMPro;
using UnityEngine;

namespace Extend {
#if UNITY_EDITOR
	[UnityEditor.InitializeOnLoad]
#endif
	internal static class StaticServiceInitializer {
#if UNITY_EDITOR
		static StaticServiceInitializer() {
			TMP_Settings settings = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_Settings>("Assets/TextMesh Pro/Res/TMP Settings.asset");
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

			Application.targetFrameRate = 60;
		}
	}
}
