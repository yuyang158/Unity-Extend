using System;
using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace Extend.Common {
	public static class UnityEngineExtensions {
		public static void RemoveSwapAt<T>(this List<T> list, int index) {
			if( index < 0 || index >= list.Count ) {
				throw new IndexOutOfRangeException();
			}

			list[index] = list[list.Count - 1];
			list.RemoveAt(list.Count - 1);
		}
		
		[LuaCallCSharp]
		public static void SetParent(this GameObject go, GameObject parent, bool stayWorld = false) {
			go.transform.SetParent(parent.transform, stayWorld);
		}

		[LuaCallCSharp]
		public static void SetParent(this GameObject go, Transform parent, bool stayWorld = false) {
			go.transform.SetParent(parent, stayWorld);
		}

		[LuaCallCSharp]
		public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component {
			return gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
		}

		[LuaCallCSharp]
		public static T GetOrAddComponent<T>(this Component component) where T : Component {
			return component.GetComponent<T>() ?? component.gameObject.AddComponent<T>();
		}
	}
}