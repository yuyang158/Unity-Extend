#if !UNITY_EDITOR
using System.Text;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using XLua;
#endif
using DG.Tweening;
using Extend.Asset;
using Extend.Common;
using Extend.DebugUtil;
using Extend.LuaUtil;
using Extend.Network;
using Extend.Network.HttpClient;
using Extend.UI.i18n;
using Extend.Render;
using Extend.SceneManagement;
using UnityEngine;
using XiaoIceland.Network;
using XiaoIceland.Service;
using XiaoIceIsland.Agora;

namespace Extend {
	internal static class StaticServiceInitializer {
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		private static void OnInitAssembliesLoaded() {
			CSharpServiceManager.Initialize();
			CSharpServiceManager.Register(new ErrorLogToFile());
			CSharpServiceManager.Register(new StatService());
			CSharpServiceManager.Register(new AssetService());
			CSharpServiceManager.Register(new GameSystemSetting());
			CSharpServiceManager.Register(new DownLoadService());
			CSharpServiceManager.Register(new AgoraService());
			#if CLOSE_UNITY_LOG
				Debug.unityLogger.logEnabled = false;
			#endif
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void OnInitBeforeSceneLoad() {
			Application.runInBackground = true;
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