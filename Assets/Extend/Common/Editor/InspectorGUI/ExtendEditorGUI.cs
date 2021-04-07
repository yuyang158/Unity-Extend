using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Extend.Common.Editor.InspectorGUI {
	[InitializeOnLoad]
	public static class ExtendEditorGUI {
		private static readonly Dictionary<Type, ExtendAttributeProcess> m_processors = new Dictionary<Type, ExtendAttributeProcess>();

		static ExtendEditorGUI() {
			foreach( var type in TypeCache.GetTypesDerivedFrom<ExtendAttributeProcess>() ) {
				var process = Activator.CreateInstance(type) as ExtendAttributeProcess;
				m_processors.Add(process.TargetAttributeType, process);
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
						if( process.Hide )
							return;
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