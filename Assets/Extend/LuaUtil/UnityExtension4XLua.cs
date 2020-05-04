using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using XLua;

namespace Extend.LuaUtil {
	[LuaCallCSharp]
	public static class UnityExtension4XLua {
		public static void SetParent(this GameObject go, GameObject parent, bool stayWorld = false) {
			go.transform.SetParent(parent.transform, stayWorld);
		}
		
		public static void SetParent(this GameObject go, Transform parent, bool stayWorld = false) {
			go.transform.SetParent(parent, stayWorld);
		}
		
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
			// ReSharper disable once LoopCanBeConvertedToQuery
			type = type.Replace('.', '/');
			foreach( var binding in bindings ) {
				if( Path.GetFileName(binding.LuaFile) == type ) {
					return binding.LuaInstance;
				}
			}

			return null;
		}
	}
}