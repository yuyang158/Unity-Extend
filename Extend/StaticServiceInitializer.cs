using ABSystem;
using Common;
using UnityEngine;

namespace Extend {
	internal static class StaticServiceInitializer {
		[RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.BeforeSceneLoad )]
		private static void OnInit() {
			CSharpServiceManager.Initialize();
			CSharpServiceManager.Register( new ABService() );
			CSharpServiceManager.Register( new LuaMVVM() );
			CSharpServiceManager.Register( new TickService() );
		}
	}
}