using System;
using Extend.Common;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Extend.AssetService {
	[Serializable]
	public class AssetReference : ISerializationCallbackReceiver {
		private AssetInstance asset;
		[SerializeField, HideInInspector]
		private Object unityObject;
		[SerializeField, HideInInspector]
		private string assetPath;
		
		public AssetReference(AssetInstance instance) {
			asset = instance;
			asset.IncRef();
		}

		public AssetReference() {
			
		}

		~AssetReference() {
			asset.Release();
		}

		public T GetAsset<T>() where T : Object {
			return asset.UnityObject as T;
		}
		
		public T Instantiate<T>(Transform parent = null, bool stayWorldPosition = false) where T : Object {
			return Object.Instantiate(asset.UnityObject, parent, stayWorldPosition) as T;
		}
		
		public T Instantiate<T>( Vector3 position, Quaternion quaternion, Transform parent = null) where T : Object {
			return Object.Instantiate(asset.UnityObject, position, quaternion, parent) as T;
		}

		public override string ToString() {
			return asset.UnityObject.name;
		}

		public void OnBeforeSerialize() {
			
		}

		public void OnAfterDeserialize() {
			if(!CSharpServiceManager.Initialized)
				return;
			
			var service = CSharpServiceManager.Get<AssetService>(CSharpServiceManager.ServiceType.ASSET_SERVICE);
			var bundle = service.TryGetAssetBundleInstance(assetPath);
			if(bundle == null)
				return;
			
			asset = new AssetInstance(string.Empty);
			asset.SetAsset(unityObject, bundle);
		}
	}
}