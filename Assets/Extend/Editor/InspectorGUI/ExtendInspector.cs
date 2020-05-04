using System;
using System.Collections.Generic;
using System.Linq;
using Extend.Common;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Extend.Editor.InspectorGUI {
	[CanEditMultipleObjects]
	[CustomEditor(typeof(Object), true)]
	public class ExtendInspector : UnityEditor.Editor {
		private readonly List<SerializedProperty> serializedProperties = new List<SerializedProperty>();
		private void GetSerializedProperties() {
			serializedProperties.Clear();
			using( var iter = serializedObject.GetIterator() ) {
				if( iter.NextVisible(true) ) {
					do {
						serializedProperties.Add(serializedObject.FindProperty(iter.name));
					} while( iter.NextVisible(false) );
				}
			}
		}

		private void DrawSerializedProperties() {
			serializedObject.Update();
			foreach( var serializedProperty in serializedProperties ) {
				ExtendEditorGUI.PropertyField_Layout(serializedProperty, true);
			}

			serializedObject.ApplyModifiedProperties();
		}

		public override void OnInspectorGUI() {
			GetSerializedProperties();
			var anyCustomAttr = serializedProperties.Any(p => p.GetAttribute<IExtendAttribute>() != null);
			if( !anyCustomAttr ) {
				DrawDefaultInspector();
			}
			else {
				DrawSerializedProperties();
			}
		}

		private void OnDisable() {
			ReorderListPropertyDrawer.Instance.ClearCache();
		}
	}
}