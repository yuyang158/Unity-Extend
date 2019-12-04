using Extend.Common;
using Extend.AssetService;
using Extend.LuaUtil;
using UnityEngine;

namespace Extend {
	internal static class StaticServiceInitializer {
		public static bool ASSET_AB_MODE = false;
		[RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.BeforeSceneLoad )]
		private static void OnInit() {
			CSharpServiceManager.Initialize();
			if( ASSET_AB_MODE ) {
				CSharpServiceManager.Register( new ABService() );
			}
			else {
				CSharpServiceManager.Register( new ResourcesService() );
			}
			CSharpServiceManager.Register( new LuaMVVM() );
			CSharpServiceManager.Register( new TickService() );
		}
	}
}