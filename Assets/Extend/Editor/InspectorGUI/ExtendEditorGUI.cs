using System;
using System.Collections.Generic;
using System.Reflection;
using Extend.Common;
using UnityEditor;
using UnityEngine;

namespace Extend.Editor.InspectorGUI {
	[InitializeOnLoad]
	public static class ExtendEditorGUI {
		private static readonly Dictionary<Type, ExtendAttributeProcess> m_processors = new Dictionary<Type, ExtendAttributeProcess>();
		static ExtendEditorGUI() {
			var types = typeof(ExtendEditorGUI).Assembly.GetTypes();
			foreach( var type in types ) {
				if( type.IsSubclassOf(typeof(ExtendAttributeProcess)) ) {
					var process = Activator.CreateInstance(type);
					m_processors.Add(type, process as ExtendAttributeProcess);
				}
			}
		}
		
		public static void PropertyField_Layout(SerializedProperty property, bool includeChildren) {
			var attributes = property.GetAttributes<IExtendAttribute>();
			var special = false;
			var label = new GUIContent(property.GetLabel());
			foreach( var attribute in attributes ) {
				if( attribute is SpecialCaseAttribute caseAttribute ) {
					special = true;
					caseAttribute.GetDrawer().OnGUI(property);
				}
				else {
					if( m_processors.TryGetValue(attribute.GetType(), out var process) ) {
						process.Process(property, label, attribute);
					}
				}
			}

			if( !special ) {
				EditorGUILayout.PropertyField(property, label, includeChildren);
			}
			
			foreach( var attribute in attributes ) {
				if( m_processors.TryGetValue(attribute.GetType(), out var process) ) {
					process.PostProcess();
				}
			}
		}
	}
}