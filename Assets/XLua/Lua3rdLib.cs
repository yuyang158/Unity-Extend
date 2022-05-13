using System;
using System.Runtime.InteropServices;
using Extend;
using Extend.Common;
using UnityEngine;

namespace XLua.LuaDLL {
	public partial class Lua {
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		private static extern int luaopen_sproto_core(IntPtr L); //[,,m]

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		private static extern int luaopen_lpeg(IntPtr L); //[,,m]

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		private static extern int luaopen_luv(IntPtr L); //[,,m]

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		private static extern int luaopen_rapidjson(IntPtr L); //[,,m]

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		private static extern void luaopen_chronos(IntPtr L);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		private static extern int luaopen_lsqlite3(IntPtr L);

#if EMMY_CORE_SUPPORT
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		private static extern int luaopen_emmy_core(IntPtr L);

		[MonoPInvokeCallback(typeof(lua_CSFunction))]
		public static int LoadEmmyCore(IntPtr L) {
			return luaopen_emmy_core(L);
		}
#endif
		
		[MonoPInvokeCallback(typeof(lua_CSFunction))]
		internal static int LoadSprotoCore(IntPtr L) {
			return luaopen_sproto_core(L);
		}

		[MonoPInvokeCallback(typeof(lua_CSFunction))]
		internal static int LoadLpeg(IntPtr L) {
			return luaopen_lpeg(L);
		}

		[MonoPInvokeCallback(typeof(lua_CSFunction))]
		internal static int LoadChronos(IntPtr L) {
			luaopen_chronos(L);
			return 1;
		}

		[MonoPInvokeCallback(typeof(lua_CSFunction))]
		internal static int LoadRapidJson(IntPtr L) {
			return luaopen_rapidjson(L);
		}

		[MonoPInvokeCallback(typeof(lua_CSFunction))]
		internal static int LoadLUV(IntPtr L) {
			return luaopen_luv(L);
		}

		[MonoPInvokeCallback(typeof(lua_CSFunction))]
		internal static int LoadLSqlite3(IntPtr L) {
			return luaopen_lsqlite3(L);
		}

		public static void OverrideLogFunction(IntPtr rawL) {
#if !XLUA_GENERAL
			lua_pushstdcallcfunction(rawL, PrintI);
			if( 0 != xlua_setglobal(rawL, "print") ) {
				throw new Exception("call xlua_setglobal fail!");
			}

			lua_pushstdcallcfunction(rawL, PrintW);
			if( 0 != xlua_setglobal(rawL, "warn") ) {
				throw new Exception("call xlua_setglobal fail!");
			}

			lua_pushstdcallcfunction(rawL, PrintE);
			if( 0 != xlua_setglobal(rawL, "error") ) {
				throw new Exception("call xlua_setglobal fail!");
			}
#endif
		}

#if !XLUA_GENERAL
		private static int CollectLog(IntPtr L, out string s) {
			s = string.Empty;
			try {
				int n = lua_gettop(L);

				if( 0 != xlua_getglobal(L, "tostring") ) {
					return luaL_error(L, "can not get tostring in print:");
				}

				for( int i = 1; i <= n; i++ ) {
					lua_pushvalue(L, -1); /* function to be called */
					lua_pushvalue(L, i); /* value to print */
					if( 0 != lua_pcall(L, 1, 1, 0) ) {
						return lua_error(L);
					}

					s += lua_tostring(L, -1);

					if( i != n ) s += "    ";

					lua_pop(L, 1); /* pop result */
				}

				return 0;
			}
			catch( Exception e ) {
				return luaL_error(L, "c# exception in print:" + e);
			}
		}

		[MonoPInvokeCallback(typeof(lua_CSFunction))]
		private static int PrintI(IntPtr L) {
			var ret = CollectLog(L, out var s);
			Debug.Log(s);
			return ret;
		}

		[MonoPInvokeCallback(typeof(lua_CSFunction))]
		private static int PrintW(IntPtr L) {
			var ret = CollectLog(L, out var s);
			Debug.LogWarning(s);
			LuaVM.LogCallStack();
			return ret;
		}

		[MonoPInvokeCallback(typeof(lua_CSFunction))]
		private static int PrintE(IntPtr L) {
			var ret = CollectLog(L, out var s);
			Debug.LogError(s);
			LuaVM.LogCallStack();
			return ret;
		}
#endif
	}
}