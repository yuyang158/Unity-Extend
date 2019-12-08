using System;
using Extend.AssetService.AssetProvider;
using UnityEngine;

namespace Extend.AssetService.AssetOperator {
	public class ABAsyncGroupOperator : AssetOperatorBase {
		private readonly string[] assetBundlePaths;
		private int loadedCount;

		public ABAsyncGroupOperator(string[] assetBundles) {
			assetBundlePaths = assetBundles;
		}

		private void CheckFinish() {
			if( loadedCount == assetBundlePaths.Length ) {
				OnComplete(this);
			}
		}

		private void OnAssetStatusChanged(AssetRefObject.AssetStatus status, AssetRefObject asset) {
			if( status == AssetRefObject.AssetStatus.DONE ) {
				loadedCount++;
				asset.OnStatusChanged -= OnAssetStatusChanged;
				CheckFinish();
			}
		}

		public override void Execute(AssetAsyncLoadHandle handle) {
			foreach( var path in assetBundlePaths ) {
				var hash = AssetBundleInstance.GenerateHash(path);
				var asset = handle.Container.TryGetAsset(hash);
				if( asset == null ) {
					throw new Exception("Logic error : " + path);
				}

				var location = AssetBundleAsyncProvider.DetermineLocation(path);
				switch( asset.Status ) {
					case AssetRefObject.AssetStatus.NONE:
					case AssetRefObject.AssetStatus.ASYNC_LOADING: {
						asset.OnStatusChanged += OnAssetStatusChanged;
						if( asset.Status != AssetRefObject.AssetStatus.NONE ) return;
						var assetBundleInstance = asset as AssetBundleInstance;
						var req = AssetBundle.LoadFromFileAsync(location);
						req.completed += _ => {
							var abProvider = handle.Provider as AssetBundleAsyncProvider;
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