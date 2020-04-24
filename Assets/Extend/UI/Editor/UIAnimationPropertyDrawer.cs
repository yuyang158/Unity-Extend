using System;
using System.Collections.Generic;
using System.Linq;
using DG.DOTweenEditor;
using DG.Tweening;
using UnityEditor;
using UnityEngine;

namespace Extend.UI.Editor {
	[CustomPropertyDrawer(typeof(UIAnimation))]
	public class UIAnimationPropertyDrawer : PropertyDrawer {
		private static readonly float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			var enabledProp = property.FindPropertyRelative("enabled");
			if( !enabledProp.boolValue )
				return lineHeight;

			var modeProp = property.FindPropertyRelative("Mode");
			var mode = (UIAnimation.AnimationMode)modeProp.intValue;
			switch( mode ) {
				case UIAnimation.AnimationMode.PUNCH:
				case UIAnimation.AnimationMode.STATE:
					var singleAnimHeight = mode == UIAnimation.AnimationMode.PUNCH ? lineHeight * 2 : lineHeight * 3;
					return lineHeight * 3 + ( singleAnimHeight + EditorGUIUtility.standardVerticalSpacing ) * animationModeActiveCount + lineHeight;
				case UIAnimation.AnimationMode.ANIMATOR:
					return lineHeight * 5;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private static readonly string[] punchTransformModeTypes = {"Move", "Rotation", "Scale"};
		private static readonly string[] stateTransformModeTypes = {"Move", "Rotation", "Scale", "Fade"};

		private static readonly Color[] transformModeColors = {
			new Color(0, 0.5f, 0, 0.4f),
			new Color(0.45f, 0.4f, 0, 0.4f),
			new Color(0.4f, 0, 0, 0.4f),
			new Color(0.4f, 0, 0.4f, 0.4f)
		};

		private static readonly GUIContent[] punchFields = {
			new GUIContent("Duration"), new GUIContent("Vibrato"),
			new GUIContent("Elasticity"), new GUIContent("Delay")
		};

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

			var mode = (UIAnimation.AnimationMode)modeProp.intValue;
			position.y += lineHeight;
			switch( mode ) {
				case UIAnimation.AnimationMode.PUNCH:
				case UIAnimation.AnimationMode.STATE:
					var previewRect = position;
					previewRect.xMax = previewRect.x + 120;
					DrawPreview(property, previewRect);
					position.y += lineHeight;

					var animationProp = property.FindPropertyRelative(mode == UIAnimation.AnimationMode.PUNCH ? "punch" : "state");
					var types = mode == UIAnimation.AnimationMode.PUNCH ? punchTransformModeTypes : stateTransformModeTypes;
					DrawAnimationMode(position, animationProp, types);

					var originLabelWidth = EditorGUIUtility.labelWidth;
					for( var i = 0; i < types.Length; i++ ) {
						var type = types[i];
						var typProp = animationProp.FindPropertyRelative(type);
						var activeProp = typProp.FindPropertyRelative("active");
						if( !activeProp.boolValue )
							continue;
						position.y += lineHeight;
						var bgColor = GUI.backgroundColor;
						if( mode == UIAnimation.AnimationMode.PUNCH )
							DrawPunchGui(ref position, typProp, i);
						else
							DrawStateGui(ref position, typProp, i);
						GUI.backgroundColor = bgColor;

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

		private static void DrawPunchGui(ref Rect position, SerializedProperty typProp, int index) {
			var punchRect = position;
			var punchProp = typProp.FindPropertyRelative("punch");
			var backgroundRect = punchRect;
			punchRect.xMax -= 5;
			backgroundRect.height = ( EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight ) * 2;
			EditorGUI.DrawRect(backgroundRect, transformModeColors[index]);
			GUI.backgroundColor = transformModeColors[index];
			EditorGUI.PropertyField(punchRect, punchProp, new GUIContent(punchTransformModeTypes[index]));
			position.y += lineHeight;
			punchRect = position;
			punchRect.xMax -= 5;
			punchRect.width *= .25f;
			EditorGUIUtility.labelWidth = punchRect.width / 2;
			var durationProp = typProp.FindPropertyRelative("duration");
			EditorGUI.PropertyField(punchRect, durationProp);
			punchRect.x = punchRect.xMax;
			var vibratoProp = typProp.FindPropertyRelative("vibrato");
			EditorGUI.PropertyField(punchRect, vibratoProp);
			punchRect.x = punchRect.xMax;
			var elasticityProp = typProp.FindPropertyRelative("elasticity");
			EditorGUI.PropertyField(punchRect, elasticityProp);
			punchRect.x = punchRect.xMax;
			var delayProp = typProp.FindPropertyRelative("delay");
			EditorGUI.PropertyField(punchRect, delayProp);
		}

		private static void DrawStateGui(ref Rect position, SerializedProperty typProp, int index) {
			var backgroundRect = position;
			backgroundRect.height = ( EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight ) * 3;
			EditorGUI.DrawRect(backgroundRect, transformModeColors[index]);
			GUI.backgroundColor = transformModeColors[index];
			var stateRect = position;
			stateRect.xMax -= 5;
			stateRect.width *= .5f;
			var originLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = stateRect.width / 2;
			var durationProp = typProp.FindPropertyRelative("duration");
			EditorGUI.PropertyField(stateRect, durationProp);
			stateRect.x = stateRect.xMax;
			var delayProp = typProp.FindPropertyRelative("delay");
			EditorGUI.PropertyField(stateRect, delayProp);
			position.y += lineHeight;
			stateRect = position;
			stateRect.xMax -= 5;
			EditorGUIUtility.labelWidth = originLabelWidth;
			var valPropName = stateTransformModeTypes[index].ToLower();
			var valProp = typProp.FindPropertyRelative(valPropName);
			EditorGUI.PropertyField(stateRect, valProp);
			position.y += lineHeight;
			stateRect = position;
			stateRect.xMax -= 5;
			var easeProp = typProp.FindPropertyRelative("ease");
			EditorGUI.PropertyField(stateRect, easeProp);
		}

		private void DrawAnimationMode(Rect transformModeSelectionRect, SerializedProperty animationProp, IReadOnlyCollection<string> types) {
			animationModeActiveCount = 0;
			transformModeSelectionRect.width /= types.Count;
			foreach( var type in types ) {
				var typProp = animationProp.FindPropertyRelative(type);
				var activeProp = typProp.FindPropertyRelative("active");

				activeProp.boolValue = EditorGUI.ToggleLeft(transformModeSelectionRect, type, activeProp.boolValue);
				transformModeSelectionRect.x = transformModeSelectionRect.xMax;
				if( activeProp.boolValue )
					animationModeActiveCount++;
			}
		}

		private static void DrawPreview(SerializedProperty property, Rect previewRect) {
			if( GUI.Button(previewRect, "Preview") ) {
				DOTweenEditorPreview.Stop(true);
				var previewGO = GameObject.Find("-[ DOTween Preview ► ]-");
				UnityEngine.Object.DestroyImmediate(previewGO);
				var previewComponent = property.serializedObject.targetObject as Behaviour;
				var field = previewComponent.GetType().GetField(property.name);
				var animation = field.GetValue(previewComponent) as IUIAnimationPreview;
				var allTween = animation.CollectPreviewTween(previewComponent.transform);
				if( allTween == null )
					return;

				foreach( var tween in allTween ) {
					if( tween == null )
						continue;

					DOTweenEditorPreview.PrepareTweenForPreview(tween);
					tween.onComplete += () => {
						if( allTween.Any(t => t != null && !t.IsComplete()) ) {
							return;
						}
						
						DOTweenEditorPreview.Stop(true);
						previewGO = GameObject.Find("-[ DOTween Preview ► ]-");
						UnityEngine.Object.DestroyImmediate(previewGO);
					};
				}

				DOTweenEditorPreview.Start();
				previewGO = GameObject.Find("-[ DOTween Preview ► ]-");
				previewGO.hideFlags |= HideFlags.HideAndDontSave;
			}

			previewRect.x = previewRect.xMax + 5;
			if( GUI.Button(previewRect, "Stop") ) {
				DOTweenEditorPreview.Stop(true);
				var previewGO = GameObject.Find("-[ DOTween Preview ► ]-");
				UnityEngine.Object.DestroyImmediate(previewGO);
			}
		}
	}
}