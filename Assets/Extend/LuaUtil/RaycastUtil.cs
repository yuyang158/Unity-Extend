using Extend.Common;
using UnityEngine;
using XLua;

namespace Extend.LuaUtil {
	[LuaCallCSharp]
	public static class RaycastUtil {
		private static readonly RaycastHit[] _hits = new RaycastHit[16];
		public static bool Raycast(Ray ray, float maxDistance, int layerMask, out RaycastHit hit) {
			return Physics.Raycast(ray, out hit, maxDistance, layerMask, QueryTriggerInteraction.Ignore);
		}

		public static bool Raycast(Vector3 origin, Vector3 direction, float maxDistance, int layerMask, out RaycastHit hit) {
			return Physics.Raycast(origin, direction, out hit, maxDistance, layerMask);
		}

		public static int RaycastAll(Vector3 origin, Vector3 direction, float maxDistance, int layerMask) {
			return Physics.RaycastNonAlloc(origin, direction, _hits, maxDistance, layerMask);
		}

		public static int CapsuleCastAll(Vector3 bottom, Vector3 top, float radius, Vector3 direction, float maxDistance, int layerMask) {
			return Physics.CapsuleCastNonAlloc(bottom, top, radius, direction, _hits, maxDistance, layerMask);
		}

		public static LuaTable GetResult(int count) {
			var luaVm = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			var ret = luaVm.NewTable();

			for( int i = 0; i < count; i++ ) {
				ret.Set(i + 1, _hits[i]);
			}

			return ret;
		}
	}
}