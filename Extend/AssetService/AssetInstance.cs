using Extend.Common;
using UnityEngine;

namespace Extend.AssetService {
	public class AssetInstance : RefObject {
		public Object UnityObject { get; }
		private ABInstance RefAB { get; }
		private string AssetPath { get; }
		
		public AssetInstance(Object unityObj, string assetPath, ABInstance refAB) {
			UnityObject = unityObj;
			if( refAB != null ) {
				RefAB = refAB;
				RefAB.IncRef();	
			}
			AssetPath = assetPath;
		}

		public override void Destroy() {
			RefAB?.Release();
			var service = CSharpServiceManager.Get<IAssetService>( CSharpServiceManager.ServiceType.ASSET_SERVICE );
			service.RemoveAsset( AssetPath );
		}
	}
}