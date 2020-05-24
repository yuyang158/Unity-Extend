using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Extend.Common;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Extend.Common.Editor.InspectorGUI {
	[CanEditMultipleObjects]
	[CustomEditor(typeof(Object), true)]
	public class ExtendInspector : UnityEditor.Editor {
		private readonly List<SerializedProperty> m_serializedProperties = new List<SerializedProperty>();

		private void GetSerializedProperties() {
			m_serializedProperties.Clear();
			using( var it = serializedObject.GetIterator() ) {
				if( it.NextVisible(true) ) {
					do {
						m_serializedProperties.Add(serializedObject.FindProperty(it.name));
					} while( it.NextVisible(false) );
				}
			}
		}

		private void DrawSerializedProperties() {
			serializedObject.Update();
			foreach( var serializedProperty in m_serializedProperties ) {
				ExtendEditorGUI.PropertyField_Layout(serializedProperty, true);
			}

			serializedObject.ApplyModifiedProperties();
		}

		public override void OnInspectorGUI() {
			GetSerializedProperties();
			var anyCustomAttr = m_serializedProperties.Any(p => p.GetAttribute<IExtendAttribute>() != null);
			if( !anyCustomAttr ) {
				DrawDefaultInspector();
			}
			else {
				DrawSerializedProperties();
			}

			var methods = target.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
			foreach( var method in methods ) {
				if( method.GetParameters().Length != 0 )
					continue;

				var attributes = method.GetCustomAttributes(typeof(ButtonAttribute), true);
				if( attributes.Length == 0 )
					continue;

				var buttonAttribute = attributes[0] as ButtonAttribute;
				var height = EditorGUIUtility.singleLineHeight;
				var buttonName = buttonAttribute.ButtonName;
				if( string.IsNullOrEmpty(buttonName) ) {
					switch( buttonAttribute.Size ) {
						case ButtonSize.Small:
							height *= 1;
							break;
						case ButtonSize.Medium:
							height *= 2;
							break;
						case ButtonSize.Large:
							height *= 3;
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}

					buttonName = method.Name;
				}

				if( GUILayout.Button(buttonName, GUILayout.Height(height)) ) {
					method.Invoke(method.IsStatic ? null : target, null);
				}
			}
		}

		protected virtual void OnDisable() {
			ReorderListPropertyDrawer.Instance.ClearCache();
		}
	}
}