using System;
using System.Collections.Generic;
using Extend.Common;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Extend.Asset {
	public class AssetPool : IDisposable {
		private int PreferSize { get; }

		private int MaxSize { get; }

		private readonly List<GameObject> m_cached;
		private readonly PrefabAssetInstance m_assetInstance;
		private float m_cacheStart;

		public Transform PoolNode { get; }

		public AssetPool(string name, int prefer, int max, PrefabAssetInstance assetInstance) {
			PreferSize = prefer;
			MaxSize = max;
			m_assetInstance = assetInstance;
			m_cached = new List<GameObject>(MaxSize);
			PoolNode = new GameObject(name).transform;
			Object.DontDestroyOnLoad(PoolNode);
			AssetService.Get().AddPool(this);
		}

		public void Cache(GameObject go) {
			StatService.Get().Increase(StatService.StatName.IN_USE_GO, -1);
			if( m_cached.Count == 0 ) {
				m_cacheStart = Time.time;
			}
			if( m_cached.Count >= MaxSize ) {
				Object.Destroy(go);
				return;
			}

			go.transform.SetParent(PoolNode, false);
			StatService.Get().Increase(StatService.StatName.IN_POOL_GO, 1);
			m_cached.Add(go);
		}

		private GameObject GetFromCache() {
			if( m_cached.Count > 0 ) {
				var go = m_cached[0];
				m_cached.RemoveSwapAt(0);
				StatService.Get().Increase(StatService.StatName.IN_POOL_GO, -1);
				return go;
			}

			return null;
		}

		public GameObject Get(Transform parent, bool stayWorldPosition) {
			var go = GetFromCache();
			if( go ) {
				go.transform.SetParent(parent, stayWorldPosition);
			}
			else {
				go = Object.Instantiate(m_assetInstance.UnityObject, parent, stayWorldPosition) as GameObject;
				var config = go.GetOrAddComponent<PoolCacheGO>();
				config.SharedPool = this;
				config.PrefabAsset = m_assetInstance;
			}

			return go;
		}
		
		public GameObject Get(Vector3 position, Quaternion quaternion, Transform parent) {
			var go = GetFromCache();
			if( go ) {
				go.transform.SetParent(parent);
				go.transform.position = position;
				go.transform.rotation = quaternion;
			}
			else {
				go = Object.Instantiate(m_assetInstance.UnityObject, position, quaternion, parent) as GameObject;
				var config = go.GetOrAddComponent<PoolCacheGO>();
				config.SharedPool = this;
				config.PrefabAsset = m_assetInstance;
			}

			return go;
		}

		public void Dispose() {
			StatService.Get().Increase(StatService.StatName.IN_POOL_GO, -m_cached.Count);
			m_cached.Clear();
			Object.Destroy(PoolNode.gameObject);
		}

		public void Update() {
			if(m_cached.Count == 0)
				return;
			
			if(m_cached.Count <= PreferSize)
				return;

			if( m_cacheStart - Time.time > 30 ) {
				Object.Destroy(m_cached[0]);
				StatService.Get().Increase(StatService.StatName.IN_POOL_GO, -1);
				m_cached.RemoveSwapAt(0);
			}
		}
	}
}