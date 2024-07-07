using UnityEngine;
using XLua;

namespace Extend.LuaUtil {
	[LuaCallCSharp]
	public static class PhysicsUtil {
		public static void ExcludeColliderLayer(Collider collider, int layerMask) {
			collider.excludeLayers |= layerMask;
		}
		public static void CancelExcludeColliderLayer(Collider collider, int layerMask) {
			collider.excludeLayers &= ~layerMask;
		}
	}
}