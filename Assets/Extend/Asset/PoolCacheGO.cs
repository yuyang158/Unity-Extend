using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Extend.Asset {
	public class PoolCacheGO : AssetServiceManagedGO {
		[SerializeField]
		private int m_preferCount = 1;

		[SerializeField]
		private int m_maxCount = 1;

		[SerializeField]
		private bool m_ignoreCacheInEditor;

		public int PreferCount => m_preferCount;

		public int MaxCount => m_maxCount;

		private WeakReference<AssetPool> m_sharedPool;
		internal AssetPool SharedPool {
			private get {
				m_sharedPool.TryGetTarget(out var pool);
				return pool;
			}
			set {
				m_sharedPool = new WeakReference<AssetPool>(value);
			}
		}

		internal override void Recycle() {
#if UNITY_EDITOR
			if( m_ignoreCacheInEditor ) {
				// Addressables.ReleaseInstance(gameObject);
				Destroy(gameObject);
				return;
			}
#endif
			SharedPool.Cache(gameObject);
			base.Recycle();
			
			Destroy(this);
		}
	}
}