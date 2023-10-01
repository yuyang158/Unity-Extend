using UnityEngine;

namespace Extend.Asset {
	[DisallowMultipleComponent]
	public abstract class AssetServiceManagedGO : MonoBehaviour {
		internal PrefabAssetInstance PrefabAsset { private get; set; }

		internal void Instantiated() {
			PrefabAsset.IncRef();
		}

		internal virtual void Recycle() {
			if( PrefabAsset.Release() < 0 ) {
				//Debug.Log(PrefabAsset);
			}
		}
	}
}
