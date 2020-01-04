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

		public AssetRefObject.AssetStatus AssetStatus => asset?.Status ?? AssetRefObject.AssetStatus.NONE;

		public AssetReference(AssetInstance instance) {
			asset = instance;
			asset?.IncRef();
		}

		public AssetReference() {
		}

		~AssetReference() {
			asset?.Release();
		}

		private T GetAsset<T>() where T : Object {
			if( asset == null ) {
				asset = AssetService.Get().LoadAssetWithGUID<T>(assetGUID);
			}
			
			Assert.AreEqual(asset.Status, AssetRefObject.AssetStatus.DONE, asset.Status.ToString());
			return asset.UnityObject as T;
		}

		public Sprite GetSprite() {
			return GetAsset<Sprite>();
		}

		public Texture GetTexture() {
			return GetAsset<Texture>();
		}
		
		public Texture3D GetTexture3D() {
			return GetAsset<Texture3D>();
		}
		
		public TextAsset GetTextAsset() {
			return GetAsset<TextAsset>();
		}
		
		public Material GetMaterial() {
			return GetAsset<Material>();
		}
		
		public GameObject GetGameObject() {
			return GetAsset<GameObject>();
		}
		
		public AudioClip GetAudioClip() {
			return GetAsset<AudioClip>();
		}
		
		public AnimationClip GetAnimationClip() {
			return GetAsset<AnimationClip>();
		}
		
		public AssetAsyncLoadHandle LoadAsync(Type typ) {
			var handle = AssetService.Get().LoadAsyncWithGUID(assetGUID, typ);
			Assert.IsNotNull(handle.Asset);
			asset = handle.Asset;
			return handle;
		}

		public GameObject Instantiate(Transform parent = null, bool stayWorldPosition = false) {
			Assert.AreEqual(asset.Status, AssetRefObject.AssetStatus.DONE);
			return Object.Instantiate(asset.UnityObject, parent, stayWorldPosition) as GameObject;
		}

		public GameObject Instantiate(Vector3 position, Quaternion quaternion, Transform parent = null) {
			Assert.AreEqual(asset.Status, AssetRefObject.AssetStatus.DONE);
			return Object.Instantiate(asset.UnityObject, position, quaternion, parent) as GameObject;
		}

		public override string ToString() {
			return (asset == null || !asset.UnityObject) ? "Not loaded" : asset.UnityObject.name;
		}
	}
}