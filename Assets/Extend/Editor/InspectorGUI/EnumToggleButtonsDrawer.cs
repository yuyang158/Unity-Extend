using System;
using Extend.Common;
using Extend.UI.Editor;
using UnityEditor;
using UnityEngine;

namespace Extend.Editor.InspectorGUI {
	[CustomPropertyDrawer(typeof(EnumToggleButtonsAttribute))]
	public class EnumToggleButtonsDrawer : PropertyDrawer {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			if( property.propertyType != SerializedPropertyType.Enum ) {
				Debug.Log($"{property.name} is not enum");
				EditorGUI.PropertyField(position, property, label);
				return;
			}
			
			var target = property.GetPropertyObject();
			var enumType = target.GetType();

			var labelRect = position;
			labelRect.width = EditorGUIUtility.labelWidth;
			EditorGUI.LabelField(labelRect, label);

			var names = Enum.GetNames(enumType);
			var toggleGroupRect = position;
			toggleGroupRect.x = position.x + EditorGUIUtility.labelWidth;

			var singleWidth = (position.width - EditorGUIUtility.labelWidth) / names.Length;
			toggleGroupRect.width = singleWidth;
			for( var i = 0; i < names.Length; i++ ) {
				var name = names[i];
				if( GUI.Button(toggleGroupRect, name, i == property.intValue ? UIEditorUtil.ButtonSelectedStyle : GUI.skin.button) ) {
					property.intValue = i;
				}

				toggleGroupRect.x += singleWidth;
			}
		}
	}
}