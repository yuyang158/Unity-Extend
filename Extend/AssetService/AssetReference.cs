using System;
using Extend.Common;
using UnityEngine;
using XLua;
using Object = UnityEngine.Object;

namespace Extend.AssetService {
	[Serializable, LuaCallCSharp]
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

		public Texture GetTexture() {
			return GetAsset<Texture>();
		}
		
		public Sprite GetSprite() {
			return GetAsset<Sprite>();
		}
		
		public GameObject GetPrefab() {
			return GetAsset<GameObject>();
		}
		
		public TextAsset GetTextAsset() {
			return GetAsset<TextAsset>();
		}
		
		public AudioClip GetAudio() {
			return GetAsset<AudioClip>();
		}

		public Material GetMaterial() {
			return GetAsset<Material>();
		}
		
		public GameObject Instantiate(Transform parent = null, bool stayWorldPosition = false) {
			return Object.Instantiate(asset.UnityObject, parent, stayWorldPosition) as GameObject;
		}
		
		public GameObject Instantiate( Vector3 position, Quaternion quaternion, Transform parent = null) {
			return Object.Instantiate(asset.UnityObject, position, quaternion, parent) as GameObject;
		}

		[BlackList]
		public override string ToString() {
			return asset.UnityObject.name;
		}

		[BlackList]
		public void OnBeforeSerialize() {
			
		}

		[BlackList]
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