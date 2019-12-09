using System.Collections;
using Extend.Common;
using UnityEngine;

namespace Extend.AssetService.AssetProvider {
	public class ResourcesLoadProvider : AssetLoadProvider {
		private static IEnumerator SimulateDelayLoad(AssetAsyncLoadHandle loadHandle) {
			var asyncReq = Resources.LoadAsync(loadHandle.Location);
			yield return asyncReq;
			var unityObject = asyncReq.asset;
			var asset = loadHandle.Container.TryGetAsset(loadHandle.AssetHashCode) as AssetInstance;
			asset.SetAsset(unityObject, null);
		}
		
		public override void ProvideAsync(AssetAsyncLoadHandle loadHandle) {
			var service = CSharpServiceManager.Get<GlobalCoroutineRunnerService>(CSharpServiceManager.ServiceType.COROUTINE_SERVICE);
			service.StartCoroutine(SimulateDelayLoad(loadHandle));
		}

		public override AssetReference Provide(string path, AssetContainer container) {
			var hash = AssetInstance.GenerateHash(path);
			var asset = container.TryGetAsset(hash) as AssetInstance;

			if( asset != null ) {
				if( asset.IsFinished ) {
					return new AssetReference(asset);
				}
			}

			var unityObject = Resources.Load<Object>(path);
			if( asset == null ) {
				asset = new AssetInstance(path);
			}
			asset.SetAsset(unityObject, null);
			return new AssetReference(asset);
		}
	}
}