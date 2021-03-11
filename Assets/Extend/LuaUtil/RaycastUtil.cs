using UnityEngine;
using XLua;

namespace Extend.LuaUtil {
	[LuaCallCSharp]
	public static class RaycastUtil {
		public static bool Raycast(Ray ray, float maxDistance, int layerMask, out RaycastHit hit) {
			return Physics.Raycast(ray, out hit, maxDistance, layerMask);
		}
	}
}