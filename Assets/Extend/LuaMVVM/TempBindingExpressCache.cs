using System.Collections.Generic;
using Extend.Common;
using XLua;

namespace Extend.LuaMVVM {
	internal static class TempBindingExpressCache {
		private const string LUA_TEMPLATE = @"return function(this) return {0} end";
		private static readonly Dictionary<int, LuaFunction> m_cachedLuaFunctions = new Dictionary<int, LuaFunction>();
		
		public static LuaFunction GenerateTempFunction(ref string script) {
			var key = script.GetHashCode();
			if( !m_cachedLuaFunctions.TryGetValue(key, out var tempFunc) ) {
				var luaVM = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
				tempFunc = luaVM.DoString(string.Format(LUA_TEMPLATE, script))[0] as LuaFunction;
				m_cachedLuaFunctions.Add(key, tempFunc);
			}

			return tempFunc;
		}
	}
}