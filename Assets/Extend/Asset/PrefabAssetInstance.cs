using System;
using Extend.Common;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Extend.Asset {
	internal class PrefabAssetInstance : AssetInstance {
		private AssetPool m_pool;
		private bool m_autoRecyclePrefab;
#if UNITY_EDITOR
		private int m_transformCount;
#endif

		public PrefabAssetInstance(AsyncOperationHandle handle) : base(handle) {
		}

#if UNITY_EDITOR
		private void ChildCountRecursive(Transform t) {
			m_transformCount += t.childCount;
			for( var i = 0; i < t.childCount; i++ ) {
				ChildCountRecursive(t.GetChild(i));
			}
		}
#endif

		protected override void SetAsset(Object unityObj) {
			if( unityObj ) {
				var prefab = unityObj as GameObject;
				var cacheConfig = prefab.GetComponent<PoolCacheGO>();
				if( cacheConfig ) {
					if( m_pool != null ) {
						throw new Exception("Pool is created!");
					}

					m_pool = new AssetPool(prefab.name, cacheConfig.PreferCount, cacheConfig.MaxCount, this);
				}

				m_autoRecyclePrefab = prefab.GetComponent<AutoRecycle>();

#if UNITY_EDITOR
				ChildCountRecursive(prefab.transform);
				m_transformCount++;
#endif
			}

			base.SetAsset(unityObj);
		}

		public void InitPool(string name, int prefer, int max) {
			if( m_pool != null )
				throw new Exception("Pool is created!");

			var go = UnityObject as GameObject;
			if( !go.GetComponent<PoolCacheGO>() ) {
				throw new Exception($"{go.name} need add PoolCacheGO component.");
			}

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

			try {
				if( m_autoRecyclePrefab ) {
					var autoRecycle = go.GetComponent<AutoRecycle>();
					autoRecycle.ResetAll();
				}
			}
			catch( Exception e ) {
				Debug.LogException(e);
			}
#if ASSET_LOG
			Debug.LogWarning($"Instantiate {UnityObject} parent : {parent}");
#endif
			StatService.Get().Increase(StatService.StatName.IN_USE_GO, 1);
#if UNITY_EDITOR
			StatService.Get().LogStat("Instantiate", UnityObject.name, m_transformCount);
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

			if( m_autoRecyclePrefab ) {
				var autoRecycle = go.GetComponent<AutoRecycle>();
				autoRecycle.ResetAll();
			}

#if ASSET_LOG
			Debug.LogWarning($"Instantiate {UnityObject} parent : {parent}");
#endif
			StatService.Get().Increase(StatService.StatName.IN_USE_GO, 1);
#if UNITY_EDITOR
			StatService.Get().LogStat("Instantiate", UnityObject.name, m_transformCount);
#endif
			return go;
		}

		internal override void Update() {
			if( Status != AssetStatus.DONE )
				return;
			m_pool.Update();
		}

		public override void Destroy() {
			base.Destroy();
			m_pool?.Dispose();
			m_pool = null;
		}
	}
}
