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
		
		public override void Execute(AssetAsyncLoadHandle handle, Type typ) {
			if( !( handle.Container.TryGetAsset(assetBundleHash) is AssetBundleInstance abInstance ) || 
			    abInstance.Status != AssetRefObject.AssetStatus.DONE ) {
				throw new Exception("Asset depend asset bundle not loaded");
			}

			if( opTarget.Status == AssetRefObject.AssetStatus.NONE ) {
				opTarget.Status = AssetRefObject.AssetStatus.ASYNC_LOADING;
				var req = abInstance.AB.LoadAssetAsync(handle.Location, typ);
				req.completed += _ => {
					opTarget.SetAsset(req.asset, abInstance);
				};
			}
		}
	}
}