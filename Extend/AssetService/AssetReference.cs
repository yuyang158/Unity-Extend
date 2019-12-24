using System;
using System.Collections;
using Extend.Common;
using UnityEngine;
using UnityEngine.Assertions;
using XLua;
using Object = UnityEngine.Object;

namespace Extend.AssetService {
	[Serializable, LuaCallCSharp]
	public class AssetReference {
		private AssetInstance asset;

		[SerializeField, HideInInspector]
		private string assetGUID;

		public AssetReference(AssetInstance instance) {
			asset = instance;
			asset.IncRef();
		}

		public AssetReference() {
		}

		~AssetReference() {
			asset?.Release();
		}

		public T GetAsset<T>() where T : Object {
			if( asset == null ) {
				asset = AssetService.Get().LoadAssetWithGUID(assetGUID);
			}
			
			return asset.UnityObject as T;
		}

		public IEnumerator LoadAsync() {
			var handle = AssetService.Get().LoadAsyncWithGUID(assetGUID);
			Assert.IsNotNull(handle.Asset);
			asset = handle.Asset;
			return handle;
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

		public GameObject Instantiate(Vector3 position, Quaternion quaternion, Transform parent = null) {
			return Object.Instantiate(asset.UnityObject, position, quaternion, parent) as GameObject;
		}

		[BlackList]
		public override string ToString() {
			return asset.UnityObject.name;
		}
	}
}