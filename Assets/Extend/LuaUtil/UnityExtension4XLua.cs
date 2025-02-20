using System;
using System.Collections.Generic;
using Extend.Common;
using Extend.UI.Fx;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using XLua;
using Object = UnityEngine.Object;

namespace Extend.LuaUtil {
	[LuaCallCSharp]
	public static class UnityExtension4XLua {
		public static LuaTable FindAllWithLuaClass(LuaTable klass) {
			var luaBindings = Object.FindObjectsByType<LuaBinding>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
			return FindInComponents(klass, luaBindings);
		}
		
		public static LuaTable FindAllWithTag(string tag) {
			var gosWithTag = GameObject.FindGameObjectsWithTag(tag);
			var luaVm = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			var t = luaVm.NewTable();
			var index = 1;
			foreach( GameObject go in gosWithTag ) {
				t.Set(index, go);
				index++;
			}

			return t;
		}
		
		public static void AppendCamera(this UniversalAdditionalCameraData cameraData, Camera camera) {
			cameraData.cameraStack.Add(camera);
		}
		
		public static void SetActive(this Component component, bool active) {
			component.gameObject.SetActive(active);
		}

		public static bool CheckObjectDestroyed(Object unityObject) {
			return unityObject;
		}

		private static readonly List<Component> m_components = new (16);
		public static int GetComponentsInChildren(Component component, Type type, bool includeInactive = false) {
			m_components.Clear();
			m_components.AddRange(component.GetComponentsInChildren(type, includeInactive));
			return m_components.Count;
		}

		public static Component GetComponentAtIndex(int index) {
			return m_components[index];
		}

		private static readonly List<LuaBinding> _bindings = new List<LuaBinding>(8);
		public static LuaTable GetLuaBinding(this GameObject go, LuaTable classMeta) {
			go.GetComponents(_bindings);
			return FindInLuaBinding(classMeta, _bindings);
		}

		public static LuaTable GetLuaBinding(this Component component, LuaTable classMeta) {
			component.GetComponents(_bindings);
			return FindInLuaBinding(classMeta, _bindings);
		}

		public static LuaTable GetLuaBindingInParent(this GameObject go, LuaTable classMeta) {
			go.GetComponentsInParent(true, _bindings);
			return FindInLuaBinding(classMeta, _bindings);
		}

		public static LuaTable GetLuaBindingInParent(this Component component, LuaTable classMeta) {
			component.GetComponentsInParent(true, _bindings);
			return FindInLuaBinding(classMeta, _bindings);
		}

		public static T AddComponent<T>(this Component component) where T : Component {
			return component.gameObject.AddComponent<T>();
		}

		public static LuaTable GetLuaBindingsInChildren(this Component component, LuaTable classMeta) {
			component.GetComponentsInChildren(true, _bindings);
			return FindInComponents(classMeta, _bindings);
		}

		public static LuaTable GetLuaBindingInChildren(this Component component, LuaTable classMeta)
		{
			var bindings = component.GetComponentsInChildren<LuaBinding>();
			return FindInLuaBinding(classMeta, bindings);
		}

		public static LuaTable GetLuaBindings(this Component component, LuaTable classMeta) {
			component.GetComponents(_bindings);
			return FindInComponents(classMeta, _bindings);
		}

		public static LuaTable GetLuaBindingsInChildren(this GameObject go, LuaTable classMeta) {
			go.GetComponentsInChildren(true, _bindings);
			return FindInComponents(classMeta, _bindings);
		}

		public static LuaTable GetLuaBindings(this GameObject go, LuaTable classMeta) {
			go.GetComponents(_bindings);
			return FindInComponents(classMeta, _bindings);
		}

		public static Component GetOrAddComponent(Component component, Type componentType) {
			return component.GetOrAddComponent(componentType);
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

		public static int GetRendererMaterialCount(this Renderer renderer) {
			return renderer.sharedMaterials.Length;
		}

		public static Material GetRendererMaterial(this Renderer renderer, int index) {
			return renderer.sharedMaterials[index];
		}
		
		public static void SetRendererMaterial(this Renderer renderer, int index, Material material) {
			var materials = renderer.sharedMaterials;
			materials[index] = material;
			renderer.sharedMaterials = materials;
		}

		private static LuaTable FindInComponents(LuaTable classMeta, IList<LuaBinding> bindings) {
			var luaVm = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			var t = luaVm.NewTable();
			var index = 1;
			foreach( var binding in bindings ) {
				if( Equals(binding.LuaClass, classMeta) || LuaVM.LuaClassCache.IsSubClassOf(binding.LuaClass, classMeta) ) {
					t.Set(index, binding.LuaInstance);
					++index;
				}
			}

			return t;
		}

		private static LuaTable FindInLuaBinding(LuaTable classMeta, IEnumerable<LuaBinding> bindings) {
			// ReSharper disable once LoopCanBeConvertedToQuery
			foreach( var binding in bindings ) {
				if( Equals(classMeta, binding.LuaClass) || LuaVM.LuaClassCache.IsSubClassOf(binding.LuaClass, classMeta) ) {
					return binding.LuaInstance;
				}
			}

			return null;
		}
	}
}