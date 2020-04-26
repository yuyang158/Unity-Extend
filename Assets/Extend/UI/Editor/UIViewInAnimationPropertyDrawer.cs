using System;
using System.Collections.Generic;
using System.Linq;
using DG.DOTweenEditor;
using DG.Tweening;
using UnityEditor;
using UnityEngine;

namespace Extend.UI.Editor {
	[CustomPropertyDrawer(typeof(UIViewInAnimation))]
	public class UIViewInAnimationPropertyDrawer : PropertyDrawer {
		private static readonly float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			var enabledProp = property.FindPropertyRelative("enabled");
			if( !enabledProp.boolValue )
				return lineHeight;

			var modeProp = property.FindPropertyRelative("Mode");
			var mode = (UIViewInAnimation.AnimationMode)modeProp.intValue;
			switch( mode ) {
				case UIViewInAnimation.AnimationMode.STATE:
					var singleAnimHeight = lineHeight * 3;
					return lineHeight * 3 + ( singleAnimHeight + EditorGUIUtility.standardVerticalSpacing ) * animationModeActiveCount + lineHeight;
				case UIViewInAnimation.AnimationMode.ANIMATOR:
					return lineHeight * 5;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private static readonly string[] stateTransformTypeNames = {"Move", "Rotate", "Scale", "Fade"};
		private static readonly string[] stateTransformModeFieldNames = {"moveInDirection", "rotateFrom", "scaleFrom", "fadeFrom"};
		

		private int animationModeActiveCount;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			position.height = EditorGUIUtility.singleLineHeight;
			var enabledProp = property.FindPropertyRelative("enabled");
			var foldRect = position;
			foldRect.xMax = foldRect.xMin + 10;
			EditorGUI.Foldout(foldRect, enabledProp.boolValue, GUIContent.none);
			var enabledRect = position;
			enabledRect.xMin += 5;
			enabledProp.boolValue = EditorGUI.Toggle(enabledRect, property.name, enabledProp.boolValue);
			if( !enabledProp.boolValue )
				return;
			position.y += lineHeight;
			position.y += EditorGUIUtility.standardVerticalSpacing;
			var modeProp = property.FindPropertyRelative("Mode");
			EditorGUI.PropertyField(position, modeProp);

			var mode = (UIViewInAnimation.AnimationMode)modeProp.intValue;
			position.y += lineHeight;
			switch( mode ) {
				case UIViewInAnimation.AnimationMode.STATE:
					var previewRect = position;
					previewRect.xMax = previewRect.x + 120;
					DrawPreview(property, previewRect);
					position.y += lineHeight;

					var animationProp = property.FindPropertyRelative("state");
					animationModeActiveCount = UIEditorUtil.DrawAnimationMode(position, animationProp, stateTransformTypeNames);

					var types = stateTransformTypeNames;
					var originLabelWidth = EditorGUIUtility.labelWidth;
					for( var i = 0; i < types.Length; i++ ) {
						var type = types[i];
						var typProp = animationProp.FindPropertyRelative(type);
						var activeProp = typProp.FindPropertyRelative("active");
						if( !activeProp.boolValue )
							continue;
						position.y += lineHeight;
						var bgColor = GUI.backgroundColor;
						DrawStateGui(ref position, typProp, i);
						GUI.backgroundColor = bgColor;

						position.y += EditorGUIUtility.standardVerticalSpacing;
						EditorGUIUtility.labelWidth = originLabelWidth;
					}

					break;
				case UIViewInAnimation.AnimationMode.ANIMATOR:
					var animatorProcessorProp = property.FindPropertyRelative("processor");
					EditorGUI.PropertyField(position, animatorProcessorProp);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		
		private static void DrawStateGui(ref Rect position, SerializedProperty typProp, int index) {
			var backgroundRect = position;
			backgroundRect.height = ( EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight ) * 3;
			EditorGUI.DrawRect(backgroundRect, UIEditorUtil.TransformModeColors[index]);
			GUI.backgroundColor = UIEditorUtil.TransformModeColors[index];
			var stateRect = position;
			stateRect.xMax -= 5;
			stateRect.width = stateRect.width * .5f - 5f;
			var originLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = stateRect.width / 2;
			var durationProp = typProp.FindPropertyRelative("duration");
			EditorGUI.PropertyField(stateRect, durationProp);
			stateRect.x = stateRect.xMax + 5;
			stateRect.width += 5;
			var delayProp = typProp.FindPropertyRelative("delay");
			EditorGUI.PropertyField(stateRect, delayProp);
			position.y += lineHeight;
			stateRect = position;
			stateRect.xMax -= 5;
			EditorGUIUtility.labelWidth = originLabelWidth;
			var valPropName = stateTransformModeFieldNames[index];
			var valProp = typProp.FindPropertyRelative(valPropName);
			EditorGUI.PropertyField(stateRect, valProp);
			position.y += lineHeight;
			stateRect = position;
			stateRect.xMax -= 5;

			if( valPropName == "rotateFrom" ) {
				var totalWidth = stateRect.width;
				stateRect.width = totalWidth * 0.65f;
				EditorGUIUtility.labelWidth = stateRect.width * 0.5f;
				var easeProp = typProp.FindPropertyRelative("ease");
				EditorGUI.PropertyField(stateRect, easeProp);

				stateRect.x = stateRect.xMax + 5;
				stateRect.width = totalWidth * 0.35f;
				stateRect.xMax -= 5;
				EditorGUIUtility.labelWidth = stateRect.width * 0.5f;
				var rotateModeProp = typProp.FindPropertyRelative("rotateMode");
				EditorGUI.PropertyField(stateRect, rotateModeProp);
				EditorGUIUtility.labelWidth = originLabelWidth;
			}
			else {
				var easeProp = typProp.FindPropertyRelative("ease");
				EditorGUI.PropertyField(stateRect, easeProp);
			}
		}
		
		private static void DrawPreview(SerializedProperty property, Rect previewRect) {
			if( GUI.Button(previewRect, "Preview") ) {
				UIEditorUtil.StartPreview(property);
			}

			previewRect.x = previewRect.xMax + 5;
			if( GUI.Button(previewRect, "Stop") ) {
				UIEditorUtil.StopPreview(property);
			}
		}
	}
}