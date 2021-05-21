using System;
using Extend.Common;
using UnityEngine;

namespace Extend.Asset {
	public class DestroyGOStatHandle : MonoBehaviour {
#if UNITY_EDITOR
		private void Awake() {
			hideFlags = HideFlags.HideInInspector;
		}
#endif
		private void OnDestroy() {
			if( !CSharpServiceManager.Initialized )
				return;
			var service = CSharpServiceManager.Get<AssetFullStatService>(CSharpServiceManager.ServiceType.ASSET_FULL_STAT);
			service.OnDestroyGameObject(gameObject);
		}
	}
}