using System;
using Extend.Asset.AssetProvider;
using UnityEngine;

namespace Extend.Asset.AssetOperator {
	public class AsyncABArrayOperator : AssetOperatorBase {
		private readonly string[] assetBundlePaths;
		private int loadedCount;

		public AsyncABArrayOperator(string[] assetBundles) {
			assetBundlePaths = assetBundles;
			if( assetBundlePaths.Length == 0 ) {
				throw new Exception("Request ab count is 0");
			}
		}

		private void CheckFinish() {
			if( loadedCount == assetBundlePaths.Length ) {
				OnComplete(this);
			}
		}

		private void OnAssetStatusChanged(AssetRefObject asset) {
			if( asset.IsFinished ) {
				loadedCount++;
				asset.OnStatusChanged -= OnAssetStatusChanged;
				CheckFinish();
			}
		}

		public override void Execute(AssetAsyncLoadHandle handle, Type typ) {
			foreach( var path in assetBundlePaths ) {
				var hash = AssetBundleInstance.GenerateHash(path);
				var asset = handle.Container.TryGetAsset(hash);
				if( asset == null ) {
					throw new Exception("Logic error : " + path);
				}

				var location = AssetBundleLoadProvider.DetermineLocation(path);
				switch( asset.Status ) {
					case AssetRefObject.AssetStatus.NONE:
					case AssetRefObject.AssetStatus.ASYNC_LOADING: {
						asset.OnStatusChanged += OnAssetStatusChanged;
						if( asset.Status != AssetRefObject.AssetStatus.NONE ) return;
						var assetBundleInstance = asset as AssetBundleInstance;
						asset.Status = AssetRefObject.AssetStatus.ASYNC_LOADING;
						var req = AssetBundle.LoadFromFileAsync(location);
						req.completed += _ => {
							var abProvider = handle.Provider as AssetBundleLoadProvider;
							assetBundleInstance.SetAssetBundle(req.assetBundle, abProvider.GetDirectDependencies(path));
						};
						break;
					}
					case AssetRefObject.AssetStatus.DONE:
						loadedCount++;
						break;
					case AssetRefObject.AssetStatus.FAIL:
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				CheckFinish();
			}
		}
	}
}