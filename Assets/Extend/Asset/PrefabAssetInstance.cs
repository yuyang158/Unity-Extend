using System;
using System.Diagnostics;
using Extend.Common;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Extend.Asset {
	public class PrefabAssetInstance : AssetInstance {
		private AssetPool m_pool;
#if UNITY_EDITOR
		private int m_transformCount;
#endif

		public PrefabAssetInstance(string assetPath) : base(assetPath) {
		}

#if UNITY_EDITOR
		private void ChildCountRecursive(Transform t) {
			m_transformCount += t.childCount;
			for( var i = 0; i < t.childCount; i++ ) {
				ChildCountRecursive(t.GetChild(i));
			}
		}
#endif

		public override void SetAsset(Object unityObj, AssetBundleInstance refAssetBundle) {
			base.SetAsset(unityObj, refAssetBundle);
			if( Status == AssetStatus.DONE ) {
				var prefab = unityObj as GameObject;
				var cacheConfig = prefab.GetComponent<PoolCacheGO>();
				if( cacheConfig ) {
					if( m_pool != null ) {
						throw new Exception("Pool is created!");
					}

					m_pool = new AssetPool(prefab.name, cacheConfig.PreferCount, cacheConfig.MaxCount, this);
				}

#if UNITY_EDITOR
				ChildCountRecursive(prefab.transform);
				m_transformCount++;
#endif
			}
		}

		public void InitPool(string name, int prefer, int max) {
			if( m_pool != null )
				throw new Exception("Pool is created!");
			m_pool = new AssetPool(name, prefer, max, this);
			m_pool.WarmUp();
		}

		public GameObject Instantiate(Transform parent, bool stayWorldPosition) {
			GameObject go;
			if( m_pool == null ) {
				go = Object.Instantiate(UnityObject, parent, stayWorldPosition) as GameObject;
				var direct = go.AddComponent<DirectDestroyGO>();
				direct.PrefabAsset = this;
				direct.Instantiated();
			}
			else {
				go = m_pool.Get(parent, stayWorldPosition);
				var cached = go.GetComponent<PoolCacheGO>();
				cached.Instantiated();
			}

			StatService.Get().Increase(StatService.StatName.IN_USE_GO, 1);
#if UNITY_EDITOR
			StatService.Get().LogStat("Instantiate", UnityObject.name, m_transformCount);
#endif

#if UNITY_DEBUG
			var service = CSharpServiceManager.Get<AssetFullStatService>(CSharpServiceManager.ServiceType.ASSET_FULL_STAT);
			service.OnInstantiateGameObject(go);
#endif
			return go;
		}

		public GameObject Instantiate(Vector3 position, Quaternion rotation, Transform parent) {
			GameObject go;
			if( m_pool == null ) {
				go = Object.Instantiate(UnityObject, position, rotation, parent) as GameObject;
				var direct = go.AddComponent<DirectDestroyGO>();
				direct.PrefabAsset = this;
				direct.Instantiated();
			}
			else {
				go = m_pool.Get(position, rotation, parent);
				var cached = go.GetComponent<PoolCacheGO>();
				cached.Instantiated();
			}

			StatService.Get().Increase(StatService.StatName.IN_USE_GO, 1);
#if UNITY_EDITOR
			StatService.Get().LogStat("Instantiate", UnityObject.name, m_transformCount);
#endif
#if UNITY_DEBUG
			var service = CSharpServiceManager.Get<AssetFullStatService>(CSharpServiceManager.ServiceType.ASSET_FULL_STAT);
			service.OnInstantiateGameObject(go);
#endif
			return go;
		}

		public override void Destroy() {
			base.Destroy();
			m_pool?.Dispose();
			m_pool = null;
		}
	}
}