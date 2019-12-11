using System;
using Extend.Common;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace Extend.AssetService {
	
	public class AssetReference {
		private readonly AssetInstance asset;
		public AssetReference(AssetInstance instance) {
			asset = instance;
			asset.IncRef();
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
	}
}