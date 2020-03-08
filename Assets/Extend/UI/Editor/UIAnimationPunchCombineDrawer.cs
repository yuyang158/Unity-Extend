using System.Collections.Generic;
using DG.DOTweenEditor;
using UnityEditor;
using UnityEngine;

namespace Extend.UI.Editor {
	public class UIAnimationPunchCombineDrawer : IUIAnimationDrawer {
		private static readonly float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
		private static readonly string[] punchTransformModeTypes = {"Move", "Rotation", "Scale"};
		private int animationModeActiveCount;
		
		private static void DrawPreview(SerializedProperty property, Rect previewRect) {
			if( GUI.Button(previewRect, "Preview") ) {
				DOTweenEditorPreview.Stop(true);
				var previewGO = GameObject.Find("-[ DOTween Preview ► ]-");
				Object.DestroyImmediate(previewGO);
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
				}

				DOTweenEditorPreview.Start();
				previewGO = GameObject.Find("-[ DOTween Preview ► ]-");
				previewGO.hideFlags |= HideFlags.HideAndDontSave;
			}

			previewRect.x = previewRect.xMax + 5;
			if( GUI.Button(previewRect, "Stop") ) {
				DOTweenEditorPreview.Stop(true);
				var previewGO = GameObject.Find("-[ DOTween Preview ► ]-");
				Object.DestroyImmediate(previewGO);
			}
		}

		public void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			var previewRect = position;
			previewRect.xMax = previewRect.x + 120;
			DrawPreview(property, previewRect);
			position.y += lineHeight;

			var animationProp = property.FindPropertyRelative("punch");
			DrawAnimationMode(position, animationProp, punchTransformModeTypes);

			var originLabelWidth = EditorGUIUtility.labelWidth;
			for( var i = 0; i < punchTransformModeTypes.Length; i++ ) {
				var type = punchTransformModeTypes[i];
				var typProp = animationProp.FindPropertyRelative(type);
				var activeProp = typProp.FindPropertyRelative("active");
				if( !activeProp.boolValue )
					continue;
				position.y += lineHeight;
				var bgColor = GUI.backgroundColor;
				/*if( mode == UIAnimation.AnimationMode.PUNCH )
					DrawPunchGui(ref position, typProp, i);
				else
					DrawStateGui(ref position, typProp, i);*/
				GUI.backgroundColor = bgColor;

				position.y += EditorGUIUtility.standardVerticalSpacing;
				EditorGUIUtility.labelWidth = originLabelWidth;
			}
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
	}
}