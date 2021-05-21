using UnityEngine;

namespace Extend.Asset {
	public class DirectDestroyGO : AssetServiceManagedGO {
#if UNITY_EDITOR
		private void Awake() {
			hideFlags = HideFlags.HideAndDontSave;
		}
#endif
		internal override void Recycle() {
			Destroy(gameObject);
			base.Recycle();
		}
	}
}