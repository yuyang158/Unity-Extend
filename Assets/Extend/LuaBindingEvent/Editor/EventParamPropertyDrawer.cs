using System;
using Extend.UI.Editor;
using UnityEditor;
using UnityEngine;

namespace Extend.LuaBindingEvent.Editor {
	[CustomPropertyDrawer(typeof(EventParam))]
	public class EventParamPropertyDrawer : PropertyDrawer {
		public override void OnGUI(Rect position, SerializedProperty paramProp, GUIContent label) {
			var totalWidth = position.width;
			position.width = totalWidth * .3f;
			var paramTypeProp = paramProp.FindPropertyRelative("Type");
			paramTypeProp.intValue = EditorGUI.Popup(position, paramTypeProp.intValue, paramTypeProp.enumDisplayNames);
			var paramType = (EventParam.ParamType)paramTypeProp.intValue;
			position.x += position.width + UIEditorUtil.ROW_CONTROL_MARGIN;
			position.width = totalWidth * .7f - UIEditorUtil.ROW_CONTROL_MARGIN;
			switch( paramType ) {
				case EventParam.ParamType.Int:
					var paramIntProp = paramProp.FindPropertyRelative("Int");
					paramIntProp.intValue = EditorGUI.IntField(position, paramIntProp.intValue);
					break;
				case EventParam.ParamType.Float:
					var paramFloatProp = paramProp.FindPropertyRelative("Float");
					paramFloatProp.floatValue = EditorGUI.FloatField(position, paramFloatProp.floatValue);
					break;
				case EventParam.ParamType.String:
					var paramStrProp = paramProp.FindPropertyRelative("Str");
					paramStrProp.stringValue = EditorGUI.TextField(position, paramStrProp.stringValue);
					break;
				case EventParam.ParamType.AssetRef:
					var assetProp = paramProp.FindPropertyRelative("AssetRef");
					EditorGUI.PropertyField(position, assetProp, GUIContent.none);
					break;
				case EventParam.ParamType.None:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}

	[CustomPropertyDrawer(typeof(BindingEvent))]
	public class BindingEventPropertyDrawer : PropertyDrawer {
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return ( EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing ) * 2;
		}
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			position.height = EditorGUIUtility.singleLineHeight;
			var funcProp = property.FindPropertyRelative("Function");
			EditorGUI.PropertyField(position, funcProp);
			position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			var paramProp = property.FindPropertyRelative("Param");
			EditorGUI.PropertyField(position, paramProp);
		}
	}
}