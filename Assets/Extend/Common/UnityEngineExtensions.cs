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

		public static bool RemoveSwap<T>(this List<T> list, T item) {
			var index = list.IndexOf(item);
			if( index < 0 ) {
				return false;
			}

			list[index] = list[list.Count - 1];
			list.RemoveAt(list.Count - 1);
			return true;
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
		public static void SetParent(this GameObject go, Component parent, bool stayWorld = false) {
			go.transform.SetParent(parent.transform, stayWorld);
		}

		public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component {
			return gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
		}

		public static T GetOrAddComponent<T>(this Component component) where T : Component {
			return component.GetComponent<T>() ?? component.gameObject.AddComponent<T>();
		}
		
		[LuaCallCSharp]
		public static Component GetOrAddComponent(this GameObject gameObject, Type componentType) {
			return gameObject.GetComponent(componentType) ?? gameObject.AddComponent(componentType);
		}

		[LuaCallCSharp]
		public static Component GetOrAddComponent(this Component component, Type componentType) {
			return component.GetComponent(componentType) ?? component.gameObject.AddComponent(componentType);
		}
	}
}