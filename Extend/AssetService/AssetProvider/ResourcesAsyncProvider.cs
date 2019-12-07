using System.Collections;
using Extend.Common;
using UnityEngine;

namespace Extend.AssetService.AssetProvider {
	public class ResourcesAsyncProvider : AssetAsyncProvider {
		private static IEnumerator SimulateDelayLoad(AssetAsyncLoadHandle loadHandle) {
			yield return new WaitForSeconds(0.35f);
			var unityObject = Resources.Load<Object>(loadHandle.Location);
			var asset = loadHandle.Container.TryGetAsset(loadHandle.AssetHashCode) as AssetInstance;
			asset.SetAsset(unityObject, null);
			yield return null;
		}
		
		public override void Provide(AssetAsyncLoadHandle loadHandle) {
			var service = CSharpServiceManager.Get<GlobalCoroutineRunnerService>(CSharpServiceManager.ServiceType.COROUTINE_SERVICE);
			service.StartCoroutine(SimulateDelayLoad(loadHandle));
		}
	}
}