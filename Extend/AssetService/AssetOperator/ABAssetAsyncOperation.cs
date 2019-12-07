using System;
using Object = UnityEngine.Object;

namespace Extend.AssetService.AssetOperator {
	public class ABAssetAsyncOperation : AssetOperatorBase {
		private readonly int assetBundleHash;
		private readonly AssetInstance opTarget;
		public ABAssetAsyncOperation(int abHash, AssetInstance asset) {
			opTarget = asset;
			assetBundleHash = abHash;
		}
		
		public override void Execute(AssetAsyncLoadHandle handle) {
			var abInstance = handle.Container.TryGetAsset(assetBundleHash) as AssetBundleInstance;
			if( abInstance == null || abInstance.Status == AssetRefObject.AssetStatus.FAIL || 
			    abInstance.Status == AssetRefObject.AssetStatus.ASYNC_LOADING || abInstance.Status == AssetRefObject.AssetStatus.NONE ) {
				throw new Exception("Asset depend asset bundle not loaded");
			}

			if( opTarget.Status == AssetRefObject.AssetStatus.NONE ) {
				opTarget.Status = AssetRefObject.AssetStatus.ASYNC_LOADING;
				var req = abInstance.AB.LoadAssetAsync<Object>(handle.Location);
				req.completed += _ => {
					if( req.asset ) {
						opTarget.SetAsset(req.asset, abInstance);
						return;
					}
					throw new Exception($"Load asset fail : {handle.Location}");
				};
			}
		}
	}
}