using System;
using System.Collections;
using System.Globalization;
using Extend.Common;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Extend.Asset.AssetProvider {
	public class ResourcesLoadProvider : AssetLoadProvider {
		private static readonly WaitForSeconds DEBUG_WAIT = new WaitForSeconds(0.2f);

		private static IEnumerator SimulateDelayLoad(AssetAsyncLoadHandle loadHandle, Type typ) {
			yield return DEBUG_WAIT;
			Object unityObject;
			if( loadHandle.Location.StartsWith("assets", true, CultureInfo.CurrentCulture) ) {
#if UNITY_EDITOR
				unityObject = UnityEditor.AssetDatabase.LoadAssetAtPath(loadHandle.Location, typ);
#else
				unityObject = null;
#endif
			}
			else {
				unityObject = Resources.Load(loadHandle.Location, typ);
			}

			loadHandle.Asset.SetAsset(unityObject, null);
		}

		public override void Initialize() {
			
		}

		public override void ProvideAsync(AssetAsyncLoadHandle loadHandle, Type typ) {
			var service = CSharpServiceManager.Get<GlobalCoroutineRunnerService>(CSharpServiceManager.ServiceType.COROUTINE_SERVICE);
			service.StartCoroutine(SimulateDelayLoad(loadHandle, typ));
		}

		public override AssetReference Provide(string path, AssetContainer container, Type typ) {
			var asset = ProvideAsset(path, container, typ);
			return new AssetReference(asset);
		}

		private static AssetInstance ProvideAsset(string path, AssetContainer container, Type typ) {
			var hash = AssetInstance.GenerateHash(path);
			if( container.TryGetAsset(hash) is AssetInstance asset && asset.IsFinished ) {
				return asset;
			}

			asset = new AssetInstance(path);
			Object unityObject;
			if( path.StartsWith("assets", true, CultureInfo.CurrentCulture) ) {
#if UNITY_EDITOR
				unityObject = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typ);
#else
				unityObject = null;
#endif
			}
			else {
				unityObject = Resources.Load(path, typ);
			}

			asset.SetAsset(unityObject, null);
			container.Put(asset);
			return asset;
		}

		internal override AssetInstance ProvideAssetWithGUID<T>(string guid, AssetContainer container, out string path) {
#if UNITY_EDITOR
			path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
			if( string.IsNullOrEmpty(path) ) {
				Debug.LogWarning($"Missing asset for guid {guid}");
				return null;
			}
			return ProvideAsset(path, container, typeof(T));
#else
			path = string.Empty;
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