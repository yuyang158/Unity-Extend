using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Extend.Switcher.Editor {
	[CustomPropertyDrawer(typeof(AnimatorSwitcher))]
	public class AnimatorSwitcherEditor : PropertyDrawer {
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			var animatorProp = property.FindPropertyRelative("Ani");
			var animator = animatorProp.objectReferenceValue as Animator;
			if( !animator ) {
				return EditorGUIUtility.singleLineHeight;
			}
			var valueProp = property.FindPropertyRelative("Value");
			var nameHashProp = valueProp.FindPropertyRelative("NameHash");
			foreach( var param in animator.parameters ) {
				if( param.nameHash == nameHashProp.intValue ) {
					switch( param.type ) {
						case AnimatorControllerParameterType.Float:
						case AnimatorControllerParameterType.Int:
						case AnimatorControllerParameterType.Bool:
							return EditorGUIUtility.singleLineHeight * 3;
						case AnimatorControllerParameterType.Trigger:
							return EditorGUIUtility.singleLineHeight * 2;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}

			return EditorGUIUtility.singleLineHeight * 2;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			position.height = EditorGUIUtility.singleLineHeight;
			var animatorProp = property.FindPropertyRelative("Ani");
			EditorGUI.ObjectField(position, animatorProp, new GUIContent("Animator"));
			position.y += EditorGUIUtility.singleLineHeight;

			var animator = animatorProp.objectReferenceValue as Animator;
			if( animator ) {
				var paramNames = animator.parameters.Select(param => param.name).ToArray();
				var valueProp = property.FindPropertyRelative("Value");
				var nameHashProp = valueProp.FindPropertyRelative("NameHash");
				var index = Array.FindIndex(paramNames, paramName => Animator.StringToHash(paramName) == nameHashProp.intValue);
				index = EditorGUI.Popup(position, "Parameter", index, paramNames);
				if(index >= 0 && index < paramNames.Length)
					nameHashProp.intValue = Animator.StringToHash(paramNames[index]);
				position.y += EditorGUIUtility.singleLineHeight;

				foreach( var param in animator.parameters ) {
					if( param.nameHash != nameHashProp.intValue ) continue;
					
					switch( param.type ) {
						case AnimatorControllerParameterType.Float:
							var floatProp = valueProp.FindPropertyRelative("fV");
							floatProp.floatValue = EditorGUI.FloatField(position, "Float", floatProp.floatValue);
							break;
						case AnimatorControllerParameterType.Int:
							var intProp = valueProp.FindPropertyRelative("iV");
							intProp.intValue = EditorGUI.IntField(position, "Int", intProp.intValue);
							break;
						case AnimatorControllerParameterType.Bool:
							var boolProp = valueProp.FindPropertyRelative("bV");
							boolProp.boolValue = EditorGUI.Toggle(position, "Bool", boolProp.boolValue);
							break;
						case AnimatorControllerParameterType.Trigger:
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
					break;
				}
			}
		}
	}
}