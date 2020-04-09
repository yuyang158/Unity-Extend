using DG.Tweening;
using Extend.Common;
using Extend.DebugUtil;
using Extend.LuaUtil;
using Extend.Network;
using UI.i18n;
using UnityEngine;

namespace Extend {
	internal static class StaticServiceInitializer {
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void OnInit() {
			DOTween.Init(true, true, LogBehaviour.Default);

			CSharpServiceManager.Initialize();
			CSharpServiceManager.Register(new StatService());
			CSharpServiceManager.Register(new ErrorLogToFile());
			CSharpServiceManager.Register(new AssetService.AssetService());
			CSharpServiceManager.Register(new AssetService.SpriteAssetService());
			CSharpServiceManager.Register(new I18nService());
			CSharpServiceManager.Register(new LuaVM());
			CSharpServiceManager.Register(new TickService());
			CSharpServiceManager.Register(new GlobalCoroutineRunnerService());
			CSharpServiceManager.Register(new NetworkService());
		}
	}
}