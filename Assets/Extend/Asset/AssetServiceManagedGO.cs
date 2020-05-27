using UnityEngine;

namespace Extend.Asset {
	[DisallowMultipleComponent]
	public abstract class AssetServiceManagedGO : MonoBehaviour {
		public PrefabAssetInstance PrefabAsset { private get; set; }

		public void Instantiated() {
			PrefabAsset.IncRef();
		}

		public virtual void Recycle() {
			PrefabAsset.Release();
		}
	}
}