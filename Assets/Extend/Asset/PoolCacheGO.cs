using UnityEngine;

namespace Extend.Asset {
	public class PoolCacheGO : AssetServiceManagedGO {
		[SerializeField]
		private int m_preferCount;

		[SerializeField]
		private int m_maxCount;

		public int PreferCount => m_preferCount;

		public int MaxCount => m_maxCount;

		public AssetPool SharedPool { private get; set; }

		internal override void Recycle() {
			SharedPool.Cache(gameObject);
			base.Recycle();
		}
	}
}