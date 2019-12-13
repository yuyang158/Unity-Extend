using System;
using UI.Animation;
using UnityEditor;
using UnityEngine;

namespace Extend.UI.Editor {
	[CustomPropertyDrawer(typeof(UIAnimation))]
	public class UIAnimationPropertyDrawer : PropertyDrawer {
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			var lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			
			var modeProp = property.FindPropertyRelative("Mode");
			var mode = (UIAnimation.AnimationMode)modeProp.intValue;
			switch( mode ) {
				case UIAnimation.AnimationMode.PUNCH:
				case UIAnimation.AnimationMode.STATE:
					return lineHeight * 2 + (lineHeight * 2 + EditorGUIUtility.standardVerticalSpacing) * animationModeActiveCount;
				case UIAnimation.AnimationMode.ANIMATOR:
					return lineHeight * 4;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private static readonly string[] transformModeTypes = { "Move", "Rotation", "Scale" };
		private static readonly Color[] transformModeColors = {new Color(0, 0.5f, 0, 0.4f), new Color(0.45f, 0.4f, 0, 0.4f), new Color(0.4f, 0, 0, 0.4f)};
		private static readonly Color[] transformModeLabelColors = {new Color(0, 1, 0), new Color(1f, 0.7f, 0), new Color(0.75f, 0, 0)};

		private static readonly GUIContent[] punchFields = {
			new GUIContent("Duration"), new GUIContent("Vibrato"),
			new GUIContent("Elasticity"), new GUIContent("Delay")
		};
		private int animationModeActiveCount;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			position.height = EditorGUIUtility.singleLineHeight;
			position.y += EditorGUIUtility.standardVerticalSpacing;
			var modeProp = property.FindPropertyRelative("Mode");
			EditorGUI.PropertyField(position, modeProp);

			var mode = (UIAnimation.AnimationMode)modeProp.intValue;
			position.y += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
			switch( mode ) {
				case UIAnimation.AnimationMode.PUNCH:
				case UIAnimation.AnimationMode.STATE:
					var transformModeSelectionRect = position;
					transformModeSelectionRect.width = position.width * 0.3333f;
					SerializedProperty animationProp = null; 
					if( mode == UIAnimation.AnimationMode.PUNCH ) {
						animationProp = property.FindPropertyRelative("punch");
					}

					animationModeActiveCount = 0;
					for( var i = 0; i < 3; i++ ) {
						var type = transformModeTypes[i];
						var typProp = animationProp.FindPropertyRelative(type);
						var activeProp = typProp.FindPropertyRelative("active");

						activeProp.boolValue = EditorGUI.ToggleLeft(transformModeSelectionRect, type, activeProp.boolValue);
						transformModeSelectionRect.x = transformModeSelectionRect.xMax;
						if(activeProp.boolValue)
							animationModeActiveCount++;
					}

					var originLabelWidth = EditorGUIUtility.labelWidth;
					for( var i = 0; i < 3; i++ ) {
						var type = transformModeTypes[i];
						var typProp = animationProp.FindPropertyRelative(type);
						var activeProp = typProp.FindPropertyRelative("active");
						if(!activeProp.boolValue)
							continue;
						position.y += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
						var punchRect = position;
						var punchProp = typProp.FindPropertyRelative("Punch");
						var backgroundRect = punchRect;
						punchRect.xMax -= 5;
						backgroundRect.height = ( EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight ) * 2;
						EditorGUI.DrawRect(backgroundRect, transformModeColors[i]);
						GUI.backgroundColor = transformModeColors[i];
						EditorGUI.PropertyField(punchRect, punchProp, new GUIContent(transformModeTypes[i]));
						position.y += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
						punchRect = position;
						punchRect.xMax -= 5;
						punchRect.width *= .25f;
						EditorGUIUtility.labelWidth = punchRect.width / 2;
						var durationProp = typProp.FindPropertyRelative("Duration");
						EditorGUI.PropertyField(punchRect, durationProp);
						punchRect.x = punchRect.xMax;
						var vibratoProp = typProp.FindPropertyRelative("Vibrato");
						EditorGUI.PropertyField(punchRect, vibratoProp);
						punchRect.x = punchRect.xMax;
						var elasticityProp = typProp.FindPropertyRelative("Elasticity");
						EditorGUI.PropertyField(punchRect, elasticityProp);
						punchRect.x = punchRect.xMax;
						var delayProp = typProp.FindPropertyRelative("Delay");
						EditorGUI.PropertyField(punchRect, delayProp);
						position.y += EditorGUIUtility.standardVerticalSpacing;
						EditorGUIUtility.labelWidth = originLabelWidth;
					}
					
					break;
				case UIAnimation.AnimationMode.ANIMATOR:
					var animatorProcessorProp = property.FindPropertyRelative("processor");
					EditorGUI.PropertyField(position, animatorProcessorProp);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}