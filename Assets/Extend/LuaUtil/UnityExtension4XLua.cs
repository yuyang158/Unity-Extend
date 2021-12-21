using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Extend.Common;
using Extend.UI.Fx;
using UnityEngine;
using XLua;

namespace Extend.LuaUtil {
	[LuaCallCSharp]
	public static class UnityExtension4XLua {
		public static void SetActive(this Component component, bool active) {
			component.gameObject.SetActive(active);
		}
		
		public static LuaTable GetLuaBinding(this GameObject go, LuaTable classMeta) {
			var bindings = go.GetComponents<LuaBinding>();
			return FindInLuaBinding(classMeta, bindings);
		}

		public static LuaTable GetLuaBinding(this Component component, LuaTable classMeta) {
			var bindings = component.GetComponents<LuaBinding>();
			return FindInLuaBinding(classMeta, bindings);
		}

		public static LuaTable GetLuaBindingInParent(this GameObject go, LuaTable classMeta) {
			var bindings = go.GetComponentsInParent<LuaBinding>();
			return FindInLuaBinding(classMeta, bindings);
		}

		public static LuaTable GetLuaBindingInParent(this Component component, LuaTable classMeta) {
			var bindings = component.GetComponentsInParent<LuaBinding>();
			return FindInLuaBinding(classMeta, bindings);
		}

		public static T AddComponent<T>(this Component component) where T : Component {
			return component.gameObject.AddComponent<T>();
		}

		public static LuaTable GetLuaBindingsInChildren(this Component component, LuaTable classMeta) {
			var bindings = component.GetComponentsInChildren<LuaBinding>();
			return FindInComponents(classMeta, bindings);
		}

		public static LuaTable GetLuaBindings(this Component component, LuaTable classMeta) {
			var bindings = component.GetComponents<LuaBinding>();
			return FindInComponents(classMeta, bindings);
		}

		public static LuaTable GetLuaBindingsInChildren(this GameObject go, LuaTable classMeta) {
			var bindings = go.GetComponentsInChildren<LuaBinding>();
			return FindInComponents(classMeta, bindings);
		}

		public static LuaTable GetLuaBindings(this GameObject go, LuaTable classMeta) {
			var bindings = go.GetComponents<LuaBinding>();
			return FindInComponents(classMeta, bindings);
		}

		public static void SetPosition(this GameObject go, float x, float y, float z) {
			go.transform.position = new Vector3(x, y, z);
		}

		public static void GetPosition(this GameObject go, out float x, out float y, out float z) {
			var position = go.transform.position;
			x = position.x;
			y = position.y;
			z = position.z;
		}

		public static void SetPosition(this Component component, float x, float y, float z) {
			component.transform.position = new Vector3(x, y, z);
		}

		public static void GetPosition(this Component component, out float x, out float y, out float z) {
			var position = component.transform.position;
			x = position.x;
			y = position.y;
			z = position.z;
		}

		public static void SetGray(this RectTransform transform, bool active) {
			var grayScale = transform.GetOrAddComponent<UIGrayScale>();
			grayScale.enabled = active;	
		}

		public static void ChangeOverrideAnimatorClip(AnimatorOverrideController controller, string clipName, AnimationClip clip) {
			controller[clipName] = clip;
		}

		private static LuaTable FindInComponents(LuaTable classMeta, LuaBinding[] bindings) {
			var luaVm = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			var t = luaVm.NewTable();
			var index = 1;
			foreach( var binding in bindings ) {
				if( Equals(binding.LuaClass, classMeta) || luaVm.LuaClassCache.IsSubClassOf(binding.LuaClass, classMeta) ) {
					t.Set(index, binding.LuaInstance);
					++index;
				}
			}

			return t;
		}

		private static LuaTable FindInLuaBinding(LuaTable classMeta, IEnumerable<LuaBinding> bindings) {
			var luaVm = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			// ReSharper disable once LoopCanBeConvertedToQuery
			foreach( var binding in bindings ) {
				if( Equals(classMeta, binding.LuaClass) || luaVm.LuaClassCache.IsSubClassOf(binding.LuaClass, classMeta) ) {
					return binding.LuaInstance;
				}
			}

			return null;
		}
	}
}