using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Extend.Asset {
	public class PoolCacheGO : AssetServiceManagedGO {
		[SerializeField]
		private int m_preferCount;

		[SerializeField]
		private int m_maxCount;

		public int PreferCount => m_preferCount;

		public int MaxCount => m_maxCount;

		private WeakReference<AssetPool> m_sharedPool;
		public AssetPool SharedPool {
			private get {
				m_sharedPool.TryGetTarget(out var pool);
				return pool;
			}
			set {
				m_sharedPool = new WeakReference<AssetPool>(value);
			}
		}

		internal override void Recycle() {
			SharedPool.Cache(gameObject);
			base.Recycle();
		}
	}
}