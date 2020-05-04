using System.Collections.Generic;
using Extend.Common;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Extend.Editor.InspectorGUI {
	public class ReorderListPropertyDrawer : SpecialCasePropertyDrawerBase {
		public static readonly ReorderListPropertyDrawer Instance = new ReorderListPropertyDrawer();
		private readonly Dictionary<string, ReorderableList> reListsByPropertyName = new Dictionary<string, ReorderableList>();

		private static string GetPropertyKeyName(SerializedProperty property) {
			return property.serializedObject.targetObject.GetInstanceID() + "/" + property.name;
		}

		protected override void OnGUI_Internal(SerializedProperty property, GUIContent label) {
			if( property.isArray ) {
				var key = GetPropertyKeyName(property);
				if( !reListsByPropertyName.TryGetValue(key, out var reList) ) {
					reList = new ReorderableList(property.serializedObject, property) {
						drawHeaderCallback = rect => { EditorGUI.LabelField(rect, label.text); },
						drawElementCallback = (rect, index, active, focused) => {
							var element = property.GetArrayElementAtIndex(index);
							EditorGUI.PropertyField(rect, element, true);
						},
						elementHeightCallback = index => EditorGUI.GetPropertyHeight(property.GetArrayElementAtIndex(index))
					};
					reListsByPropertyName[key] = reList;
				}
				reList.DoLayoutList();
			}
			else {
				const string message = nameof(ReorderListAttribute) + " can be used only on arrays or lists";
				EditorGUILayout.HelpBox(message, MessageType.Error);
			}
		}

		public void ClearCache() {
			reListsByPropertyName.Clear();
		}
	}
}