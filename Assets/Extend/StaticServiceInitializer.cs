using System.IO;
using DG.Tweening;
using Extend.Asset;
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
			CSharpServiceManager.Register(new GlobalCoroutineRunnerService());
			CSharpServiceManager.Register(new StatService());
			CSharpServiceManager.Register(new ErrorLogToFile());
			CSharpServiceManager.Register(new AssetService());
			using( var asset = AssetService.Get().Load("Config/SystemSetting", typeof(TextAsset)) ) {
				using( var reader = new StringReader(asset.GetTextAsset().text) ) {
					IniRead.Parse(reader);
				}
			}

			CSharpServiceManager.Register(new SpriteAssetService());
			CSharpServiceManager.Register(new I18nService());
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void OnSceneLoaded() {
			CSharpServiceManager.Register(new LuaVM());
			CSharpServiceManager.Register(new TickService());
			CSharpServiceManager.Register(new NetworkService());

			var service = CSharpServiceManager.Instance;
			CSharpServiceManager.Register(service.gameObject.AddComponent<InGameConsole>());

			Application.targetFrameRate = 60;
		}
	}
}