using System.IO;
using Extend.Common.Lua;
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
		
		public static ILuaTable GetLuaBinding(this GameObject go, string type) {
			var bindings = go.GetComponents<LuaBinding>();
			return FindInLuaBinding(type, bindings);
		}

		public static ILuaTable GetLuaBinding(this Component component, string type) {
			var bindings = component.GetComponents<LuaBinding>();
			return FindInLuaBinding(type, bindings);
		}
		
		public static T AddComponent<T>(this Component component) where T : Component {
			return component.gameObject.AddComponent<T>();
		}

		private static ILuaTable FindInLuaBinding(string type, LuaBinding[] bindings) {
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