using System;
using Object = UnityEngine.Object;

namespace Extend.AssetService.AssetOperator {
	public class AsyncABAssetOperator : AssetOperatorBase {
		private readonly int assetBundleHash;
		private readonly AssetInstance opTarget;
		public AsyncABAssetOperator(int abHash, AssetInstance asset) {
			opTarget = asset;
			assetBundleHash = abHash;
		}
		
		public override void Execute(AssetAsyncLoadHandle handle) {
			if( !( handle.Container.TryGetAsset(assetBundleHash) is AssetBundleInstance abInstance ) || abInstance.Status == AssetRefObject.AssetStatus.FAIL || 
				abInstance.Status == AssetRefObject.AssetStatus.ASYNC_LOADING || abInstance.Status == AssetRefObject.AssetStatus.NONE ) {
				throw new Exception("Asset depend asset bundle not loaded");
			}

			if( opTarget.Status == AssetRefObject.AssetStatus.NONE ) {
				opTarget.Status = AssetRefObject.AssetStatus.ASYNC_LOADING;
				var req = abInstance.AB.LoadAssetAsync<Object>(handle.Location);
				req.completed += _ => {
					opTarget.SetAsset(req.asset, abInstance);
				};
			}
		}
	}
}