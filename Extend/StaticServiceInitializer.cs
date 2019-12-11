using Extend.Common;
using Extend.AssetService;
using Extend.LuaUtil;
using UnityEngine;

namespace Extend {
	internal static class StaticServiceInitializer {
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void OnInit() {
			CSharpServiceManager.Initialize();
			CSharpServiceManager.Register(new AssetService.AssetService());
			CSharpServiceManager.Register(new TickService());
			CSharpServiceManager.Register(new GlobalCoroutineRunnerService());
			CSharpServiceManager.Register(new LuaMVVM.LuaMVVM());
		}
	}
}