using System.IO;
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

		private static LuaTable FindInLuaBinding(string type, LuaBinding[] bindings) {
			type = type.Replace('.', '/');
			// ReSharper disable once LoopCanBeConvertedToQuery
			foreach( var binding in bindings ) {
				if( Path.GetFileName(binding.LuaFile) == type ) {
					return binding.LuaInstance;
				}
			}

			return null;
		}
	}
}