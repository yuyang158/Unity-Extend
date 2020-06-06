using System;
using UnityEngine;

namespace Extend.Asset {
	[DisallowMultipleComponent]
	public abstract class AssetServiceManagedGO : MonoBehaviour {
		public PrefabAssetInstance PrefabAsset { private get; set; }
		private bool m_recycled;

		internal void Instantiated() {
			m_recycled = false;
			PrefabAsset.IncRef();
		}

		internal virtual void Recycle() {
			PrefabAsset.Release();
			m_recycled = true;
		}

		private void OnDestroy() {
			if( !m_recycled ) {
				PrefabAsset.Release();
			}
		}
	}
}