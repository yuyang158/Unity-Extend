using System.Reflection;
using Extend.Common;
using UnityEditor;
using UnityEngine;

namespace Extend.Editor.InspectorGUI {
	public static class ExtendEditorGUI {
		public static void PropertyField_Layout(SerializedProperty property, bool includeChildren) {
			var specialCastAttribute = property.GetAttribute<SpecialCaseAttribute>();
			if( specialCastAttribute != null ) {
				specialCastAttribute.GetDrawer().OnGUI(property);
			}
			else {
				var target = property.GetPropertyParentObject();
				var hideAttr = property.GetAttribute<HideIfAttribute>();
				if( hideAttr != null ) {
					var fieldInfo = target.GetType().GetField(hideAttr.FieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					var val = fieldInfo.GetValue(target);
					if(val == hideAttr.Value)
						return;
				}
				
				var showAttr = property.GetAttribute<ShowIfAttribute>();
				if( showAttr != null ) {
					var fieldInfo = target.GetType().GetField(showAttr.FieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					var val = fieldInfo.GetValue(target);
					if(val != showAttr.Value)
						return;
				}

				var enableAttr = property.GetAttribute<EnableIfAttribute>();
				if( enableAttr != null ) {
					var fieldInfo = target.GetType().GetField(enableAttr.FieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					var val = fieldInfo.GetValue(target);
					if( val != enableAttr.Value ) {
						GUI.enabled = false;
					}
				}

				var requireAttr = property.GetAttribute<RequireAttribute>();
				if( requireAttr != null ) {
					if( property.propertyType == SerializedPropertyType.String ) {
						if( string.IsNullOrEmpty(property.stringValue) ) {
							EditorGUILayout.HelpBox($"{property.name} is required", MessageType.Error);
						}
					}
					else if( property.propertyType == SerializedPropertyType.ObjectReference ) {
						if( !property.objectReferenceValue ) {
							EditorGUILayout.HelpBox($"{property.name} is required", MessageType.Error);
						}
					}
					else {
						Debug.LogError($"only string or unity object type support require attribute");
					}
				}
				
				var label = new GUIContent(property.GetLabel());
				EditorGUILayout.PropertyField(property, label, includeChildren);

				if( enableAttr != null ) {
					GUI.enabled = true;
				}
			}
		}
	}
}