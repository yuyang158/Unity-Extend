﻿using System.Collections.Generic;
using Extend.Common;
using XLua;

namespace Extend.LuaMVVM {
	internal static class TempBindingExpressCache {
		private const string LUA_TEMPLATE = @"return function(this, current) return {0} end";
		private static readonly Dictionary<int, LuaFunction> m_cachedLuaFunctions = new Dictionary<int, LuaFunction>();
		
		public static LuaFunction GenerateTempFunction(ref string script) {
			var key = script.GetHashCode();
			if( !m_cachedLuaFunctions.TryGetValue(key, out var tempFunc) ) {
				var luaVM = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
				tempFunc = luaVM.DoBindingString(string.Format(LUA_TEMPLATE, script), script)[0] as LuaFunction;
				m_cachedLuaFunctions.Add(key, tempFunc);
			}

			return tempFunc;
		}

		public static void Clear() {
			foreach( var func in m_cachedLuaFunctions.Values ) {
				func.Dispose();
			}
			m_cachedLuaFunctions.Clear();
		}
	}
}