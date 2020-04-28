using System;
using System.Collections.Generic;
using System.Linq;
using DG.DOTweenEditor;
using DG.Tweening;
using UnityEditor;
using UnityEngine;

namespace Extend.UI.Editor {
	public abstract class UIAnimationPropertyBaseDrawer : PropertyDrawer {
		protected static readonly float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
		protected SerializedProperty animationProperty;
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			animationProperty = property;
			var enabledProp = property.FindPropertyRelative("enabled");
			if( !enabledProp.boolValue )
				return lineHeight;

			var modeProp = property.FindPropertyRelative("Mode");
			var mode = modeProp.intValue;
			if( mode == 0 ) {
				return lineHeight * 5;
			}
			return lineHeight * 3 + ( SingleDoTweenHeight + EditorGUIUtility.standardVerticalSpacing ) * animationModeActiveCount + lineHeight;
		}

		private int animationModeActiveCount;
		protected abstract UIAnimationParamCombine[] CurrentAnimation { get; }
		protected abstract float SingleDoTweenHeight { get; }
		protected abstract string[] Mode { get; }

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			animationProperty = property;
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

			var mode = (UIViewLoopAnimation.AnimationMode)modeProp.intValue;
			position.y += lineHeight;
			switch( mode ) {
				case UIViewLoopAnimation.AnimationMode.STATE:
					var previewRect = position;
					previewRect.xMax = previewRect.x + 120;
					DrawPreview(property, previewRect);
					position.y += lineHeight;

					var animationProp = property.FindPropertyRelative("state");
					animationModeActiveCount = UIEditorUtil.DrawAnimationMode(position, animationProp, Mode);
					position.y += lineHeight;
					var originLabelWidth = EditorGUIUtility.labelWidth;
					for( var i = 0; i < Mode.Length; i++ ) {
						var type = Mode[i];
						var typProp = animationProp.FindPropertyRelative(type);
						var activeProp = typProp.FindPropertyRelative("active");
						if( !activeProp.boolValue )
							continue;
						position = CurrentAnimation[i].OnGUI(position, typProp);
						EditorGUIUtility.labelWidth = originLabelWidth;
					}

					break;
				case UIViewLoopAnimation.AnimationMode.ANIMATOR:
					var animatorProcessorProp = property.FindPropertyRelative("processor");
					EditorGUI.PropertyField(position, animatorProcessorProp);
					break;
				default:
					throw new ArgumentOutOfRangeException();
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