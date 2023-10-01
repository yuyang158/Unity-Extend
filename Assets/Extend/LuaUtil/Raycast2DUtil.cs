using Extend.Common;
using UnityEngine;
using XLua;

namespace Extend.LuaUtil {
	[LuaCallCSharp]
	public static class Raycast2DUtil {
		public static Collider2D OverlapPoint(Vector2 point) {
			var collider = Physics2D.OverlapPoint(point);
			return collider;
		}

		public static LuaTable GetResult(int count, Collider[] colliders) {
			var luaVm = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			var ret = luaVm.NewTable();

			for( int i = 0; i < count; i++ ) {
				ret.Set(i + 1, colliders[i]);
			}

			return ret;
		}
	}
}
