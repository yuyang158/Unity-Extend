using System.Collections.Generic;
using System.IO;
using Extend.Common;
using UnityEngine;
using XLua;

namespace Extend.LuaUtil {
	[LuaCallCSharp]
	public static class UnityExtension4XLua {
		public static LuaTable GetLuaBinding(this GameObject go, string type) {
			var bindings = go.GetComponents<LuaBinding>();
			return FindInLuaBinding(type, bindings);
		}

		public static LuaTable GetLuaBinding(this Component component, string type) {
			var bindings = component.GetComponents<LuaBinding>();
			return FindInLuaBinding(type, bindings);
		}
		
		public static T AddComponent<T>(this Component component) where T : Component {
			return component.gameObject.AddComponent<T>();
		}
		
		public static LuaTable GetLuaBindingsInChildren(this Component component, string type) {
			var bindings = component.GetComponentsInChildren<LuaBinding>();
			return FindInComponents(type, bindings);
		}

		public static LuaTable GetLuaBindings(this Component component, string type) {
			var bindings = component.GetComponents<LuaBinding>();
			return FindInComponents(type, bindings);
		}
		
		public static LuaTable GetLuaBindingsInChildren(this GameObject go, string type) {
			var bindings = go.GetComponentsInChildren<LuaBinding>();
			return FindInComponents(type, bindings);
		}

		public static LuaTable GetLuaBindings(this GameObject go, string type) {
			var bindings = go.GetComponents<LuaBinding>();
			return FindInComponents(type, bindings);
		}

		private static LuaTable FindInComponents(string type, LuaBinding[] bindings) {
			var luaVm = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			var t = luaVm.NewTable();
			var index = 1;
			foreach( var binding in bindings ) {
				if( binding.LuaFile == type ) {
					t.Set(index, binding.LuaInstance);
					++index;
				}
			}

			return t;
		}

		private static LuaTable FindInLuaBinding(string type, IEnumerable<LuaBinding> bindings) {
			// ReSharper disable once LoopCanBeConvertedToQuery
			foreach( var binding in bindings ) {
				if( binding.LuaFile.StartsWith(type) ) {
					return binding.LuaInstance;
				}
			}

			return null;
		}
	}
}