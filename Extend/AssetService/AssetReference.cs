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
			
			Assert.AreEqual(asset.Status, AssetRefObject.AssetStatus.DONE);
			return asset.UnityObject as T;
		}

		public AssetAsyncLoadHandle LoadAsync() {
			var handle = AssetService.Get().LoadAsyncWithGUID(assetGUID);
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
			return (asset == null || !asset.UnityObject) ? "Empty" : asset.UnityObject.name;
		}
	}

	[Serializable, LuaCallCSharp]
	public class AssetReferenceT<T> : AssetReference where T : Object {
		
	}
}