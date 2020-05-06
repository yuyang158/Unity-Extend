using System;
using System.Collections;
using System.Reflection;
using Extend.Common;
using UnityEditor;

namespace Extend.Editor.InspectorGUI {
	public static class SerializePropertyExtend {
		private static object GetValue_Imp(object source, string name) {
			if( source == null ) {
				return null;
			}

			var type = source.GetType();
			while( type != null ) {
				var field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
				if( field != null ) {
					return field.GetValue(source);
				}

				var property = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
				if( property != null ) {
					return property.GetValue(source, null);
				}

				type = type.BaseType;
			}

			return null;
		}

		private static object GetValue_Imp(object source, string name, int index) {
			if( !( GetValue_Imp(source, name) is IEnumerable enumerable ) ) {
				return null;
			}

			var enumerator = enumerable.GetEnumerator();
			for( var i = 0; i <= index; i++ ) {
				if( !enumerator.MoveNext() ) {
					return null;
				}
			}

			return enumerator.Current;
		}

		public static object GetTargetObjectWithProperty(this SerializedProperty property) {
			var path = property.propertyPath.Replace(".Array.data[", "[");
			object obj = property.serializedObject.targetObject;
			var elements = path.Split('.');

			for( var i = 0; i < elements.Length; i++ ) {
				var element = elements[i];
				if( element.Contains("[") ) {
					var elementName = element.Substring(0, element.IndexOf("["));
					var index = Convert.ToInt32(element.Substring(element.IndexOf("[", StringComparison.CurrentCulture)).Replace("[", "").Replace("]", ""));
					obj = GetValue_Imp(obj, elementName, index);
				}
				else {
					obj = GetValue_Imp(obj, element);
				}
			}

			return obj;
		}

		public static T[] GetAttributes<T>(this SerializedProperty p) where T : class {
			var fieldInfo = ReflectionUtility.GetField(p.GetTargetObjectWithProperty(), p.name);
			if( fieldInfo == null ) {
				return new T[] {};
			}

			return (T[])fieldInfo.GetCustomAttributes(typeof(T), true);
		}
		
		public static T GetAttribute<T>(this SerializedProperty p) where T : class {
			var attrs = p.GetAttributes<T>();
			return attrs.Length == 0 ? null : attrs[0];
		}

		public static string GetLabel(this SerializedProperty p) {
			var labelText = p.GetAttribute<LabelTextAttribute>();
			return labelText == null ? p.displayName : labelText.Text;
		} 
	}
}