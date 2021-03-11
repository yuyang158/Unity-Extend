using Extend.Common;
using UnityEngine;

namespace Extend.Asset {
	public class DestroyGOStatHandle : MonoBehaviour {
		private void OnDestroy() {
			if( !CSharpServiceManager.Initialized )
				return;
			var service = CSharpServiceManager.Get<AssetFullStatService>(CSharpServiceManager.ServiceType.ASSET_FULL_STAT);
			service.OnDestroyGameObject(gameObject);
		}
	}
}