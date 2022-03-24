using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Extend.Asset {
	public class DirectDestroyGO : AssetServiceManagedGO {
#if UNITY_EDITOR
		private void Awake() {
			hideFlags = HideFlags.HideAndDontSave;
		}
#endif
		internal override void Recycle() {
			transform.SetParent(null);
			Destroy(gameObject);
			// Addressables.ReleaseInstance(gameObject);
			base.Recycle();
		}
	}
}