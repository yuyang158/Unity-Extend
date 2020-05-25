using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Extend.Asset {
	public class PrefabAssetInstance : AssetInstance {
		private List<GameObject> m_inUsedGo = new List<GameObject>();
		private AssetPool m_pool;
		
		public PrefabAssetInstance(string assetPath) : base(assetPath) {
		}

		public override void SetAsset(Object unityObj, AssetBundleInstance refAssetBundle) {
			base.SetAsset(unityObj, refAssetBundle);
			if( Status == AssetStatus.DONE ) {
				var prefab = unityObj as GameObject;
				var cacheConfig = prefab.GetComponent<AssetCacheConfig>();
				if( cacheConfig ) {
					if( m_pool != null ) {
						throw new Exception("Pool is created!");
					}
					m_pool = new AssetPool(prefab.name, cacheConfig.PreferCount, cacheConfig.MaxCount);
				}
			}
		}

		public void InitPool(string name, int prefer, int max) {
			if(m_pool != null)
				throw new Exception("Pool is created!");
			m_pool = new AssetPool(name, prefer, max);
		}

		public GameObject Instantiate(Transform parent, bool stayWorldPosition) {
			GameObject go;
			if( m_pool == null ) {
				go = Object.Instantiate(UnityObject, parent, stayWorldPosition) as GameObject;
				return go;
			}
			go = m_pool.Get();
			if( go ) {
				go.transform.SetParent(parent, stayWorldPosition);
			}
			else {
				go = Object.Instantiate(UnityObject, parent, stayWorldPosition) as GameObject;
				var cache = go.GetComponent<AssetCacheConfig>();
				cache.Pool = m_pool;
			}
			return go;
		}

		public GameObject Instantiate(Vector3 position, Quaternion quaternion, Transform parent) {
			GameObject go;
			if( m_pool == null ) {
				go = Object.Instantiate(UnityObject, position, quaternion, parent) as GameObject;
				return go;
			}
			go = m_pool.Get();
			if( go ) {
				go.transform.SetParent(parent, false);
				go.transform.localPosition = position;
				go.transform.localRotation = quaternion;
			}
			else {
				go = Object.Instantiate(UnityObject, position, quaternion, parent) as GameObject;
				var cache = go.GetComponent<AssetCacheConfig>();
				cache.Pool = m_pool;
			}
			return go;
		}

		public void Recycle(GameObject go) {
			m_pool.Cache(go);
		}

		public override void Destroy() {
			base.Destroy();
			m_pool?.Dispose();
			m_pool = null;
		}
	}
}