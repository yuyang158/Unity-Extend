using System;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Extend.Common.Editor {
	[CustomPropertyDrawer(typeof(AnimatorParamProcessor))]
	public class AnimatorParamProcessorDrawer : PropertyDrawer {
		private static readonly GUIContent AnimatorName = new GUIContent("Animator");

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			var animatorProp = property.FindPropertyRelative("m_ani");
			var animator = animatorProp.objectReferenceValue as Animator;
			if( !animator ) {
				return UIEditorUtil.LINE_HEIGHT;
			}

			if( !animator.runtimeAnimatorController ) {
				return UIEditorUtil.LINE_HEIGHT;
			}

			var valueProp = property.FindPropertyRelative("m_paramValue");
			var nameHashProp = valueProp.FindPropertyRelative("NameHash");
			foreach( var param in animator.parameters ) {
				if( param.nameHash == nameHashProp.intValue ) {
					switch( param.type ) {
						case AnimatorControllerParameterType.Float:
						case AnimatorControllerParameterType.Int:
						case AnimatorControllerParameterType.Bool:
							return UIEditorUtil.LINE_HEIGHT * 3;
						case AnimatorControllerParameterType.Trigger:
							return UIEditorUtil.LINE_HEIGHT * 2;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}

			return UIEditorUtil.LINE_HEIGHT * 2;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			position.height = EditorGUIUtility.singleLineHeight;
			var animatorProp = property.FindPropertyRelative("m_ani");
			EditorGUI.ObjectField(position, animatorProp, AnimatorName);
			position.y += UIEditorUtil.LINE_HEIGHT;

			var animator = animatorProp.objectReferenceValue as Animator;
			if( !animator )
				return;
			if( !animator.runtimeAnimatorController ) {
				return;
			}

			var paramNames = new string[animator.parameters.Length];
			for( int i = 0; i < animator.parameters.Length; i++ ) {
				var parameter = animator.parameters[i];
				paramNames[i] = parameter.name;
			}

			var valueProp = property.FindPropertyRelative("m_paramValue");
			var nameHashProp = valueProp.FindPropertyRelative("NameHash");
			var index = Array.FindIndex(paramNames, paramName => Animator.StringToHash(paramName) == nameHashProp.intValue);
			index = EditorGUI.Popup(position, "Parameter", index, paramNames);
			if( index >= 0 && index < paramNames.Length )
				nameHashProp.intValue = Animator.StringToHash(paramNames[index]);
			position.y += UIEditorUtil.LINE_HEIGHT;

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