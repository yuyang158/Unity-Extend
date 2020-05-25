using System;
using UnityEngine;

namespace Extend.Asset {
	public class AssetCacheConfig : MonoBehaviour {
		[SerializeField]
		private int m_preferCount;

		[SerializeField]
		private int m_maxCount;

		public int PreferCount => m_preferCount;

		public int MaxCount => m_maxCount;

		public AssetPool Pool;
	}
}