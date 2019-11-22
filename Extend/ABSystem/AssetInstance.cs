using Common;
using UnityEngine;

namespace ABSystem {
	public class AssetInstance : RefObject {
		public Object UnityObject { get; }
		private ABInstance RefAB { get; }
		private string AssetPath { get; }
		
		public AssetInstance(Object unityObj, string assetPath, ABInstance refAB) {
			UnityObject = unityObj;
			RefAB = refAB;
			RefAB.IncRef();
			AssetPath = assetPath;
		}

		public override void Destroy() {
			RefAB.Release();
			var service = CSharpServiceManager.Get<ABService>( CSharpServiceManager.ServiceType.AB_SERVICE );
			service.RemoveAsset( AssetPath );
		}
	}
}