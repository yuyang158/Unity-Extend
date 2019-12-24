using System.Collections;
using Extend.Common;
using UnityEngine;

namespace Extend.AssetService.AssetProvider {
	public class ResourcesLoadProvider : AssetLoadProvider {
		private static IEnumerator SimulateDelayLoad(AssetAsyncLoadHandle loadHandle) {
			var asyncReq = Resources.LoadAsync(loadHandle.Location);
			yield return asyncReq;
			var unityObject = asyncReq.asset;
			loadHandle.Asset.SetAsset(unityObject, null);
		}
		
		public override void ProvideAsync(AssetAsyncLoadHandle loadHandle) {
			var service = CSharpServiceManager.Get<GlobalCoroutineRunnerService>(CSharpServiceManager.ServiceType.COROUTINE_SERVICE);
			service.StartCoroutine(SimulateDelayLoad(loadHandle));
		}

		public override AssetReference Provide(string path, AssetContainer container) {
			var asset = ProvideAsset(path, container);
			return new AssetReference(asset);
		}

		internal override AssetInstance ProvideAsset(string path, AssetContainer container) {
			var hash = AssetInstance.GenerateHash(path);
			if( container.TryGetAsset(hash) is AssetInstance asset && asset.IsFinished ) {
				return asset;
			}
			asset = new AssetInstance(path);
			var unityObject = UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(path);
			asset.SetAsset(unityObject, null);
			container.Put(asset);
			return asset;
		}
		
		internal override AssetInstance ProvideAssetWithGUID(string guid, AssetContainer container) {
#if UNITY_EDITOR
			var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
			return ProvideAsset(path, container);
#else
			return null;
#endif
		}

		internal override string ConvertGUID2Path(string guid) {
#if UNITY_EDITOR
			return UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
#else
			return null;
#endif
		}
	}
}