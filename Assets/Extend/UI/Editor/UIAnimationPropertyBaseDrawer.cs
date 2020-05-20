using System.Collections.Generic;
using System.Linq;
using DG.DOTweenEditor;
using DG.Tweening;
using UnityEditor;
using UnityEngine;

namespace Extend.UI.Editor {
	public abstract class UIAnimationPropertyBaseDrawer : PropertyDrawer {
		protected SerializedProperty animationProperty;
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			animationProperty = property;
			var height = UIEditorUtil.LINE_HEIGHT;
			var enabledProp = property.FindPropertyRelative("m_enabled");
			if( !enabledProp.boolValue )
				return height;

			height += 2 * UIEditorUtil.LINE_HEIGHT;

			var modeProp = property.FindPropertyRelative("Mode");
			var mode = modeProp.intValue;
			if( mode == 0 ) {
				height += 5 * UIEditorUtil.LINE_HEIGHT;
			}
			else {
				height += UIEditorUtil.LINE_HEIGHT * 4 + ( SingleDoTweenHeight + EditorGUIUtility.standardVerticalSpacing ) 
				          * animationModeActiveCount;
			}
			return height;
		}

		private bool animationOrTriggerMode = true;
		

		private int animationModeActiveCount;
		protected abstract UIAnimationParamCombine[] CurrentAnimation { get; }
		protected abstract float SingleDoTweenHeight { get; }
		protected abstract string[] Mode { get; }

		protected virtual string GetAnimationFieldName(int mode) {
			return "m_state";
		}
		
		private static int DrawAnimationMode(Rect transformModeSelectionRect, SerializedProperty animationProp, IReadOnlyCollection<string> types) {
			var animationModeActiveCount = 0;
			transformModeSelectionRect.width /= types.Count;
			foreach( var type in types ) {
				var typProp = animationProp.FindPropertyRelative(type);
				if( typProp == null ) {
					Debug.LogError($"{type}");
					continue;
				}
				var activeProp = typProp.FindPropertyRelative("m_active");
				activeProp.boolValue = EditorGUI.ToggleLeft(transformModeSelectionRect, type, activeProp.boolValue);
				transformModeSelectionRect.x = transformModeSelectionRect.xMax;
				if( activeProp.boolValue )
					animationModeActiveCount++;
			}

			return animationModeActiveCount;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			animationProperty = property;
			position.height = EditorGUIUtility.singleLineHeight;
			var enabledProp = property.FindPropertyRelative("m_enabled");
			var foldRect = position;
			foldRect.xMax = foldRect.xMin + 10;
			EditorGUI.Foldout(foldRect, enabledProp.boolValue, GUIContent.none);
			var enabledRect = position;
			enabledRect.xMin += 5;
			enabledProp.boolValue = EditorGUI.Toggle(enabledRect, property.name, enabledProp.boolValue);
			if( !enabledProp.boolValue )
				return;

			position.y += UIEditorUtil.LINE_HEIGHT;
			var splitRect = position;
			splitRect.height = UIEditorUtil.LINE_HEIGHT * 2 - EditorGUIUtility.standardVerticalSpacing;
			var width = splitRect.width / 2;
			splitRect.width = width;
			if( GUI.Button(splitRect, "Animation", animationOrTriggerMode ? UIEditorUtil.ButtonSelectedStyle : GUI.skin.button) ) {
				animationOrTriggerMode = true;
			}
			splitRect.x = splitRect.xMax;
			if( GUI.Button(splitRect, "OnTrigger", animationOrTriggerMode ? GUI.skin.button : UIEditorUtil.ButtonSelectedStyle) ) {
				animationOrTriggerMode = false;
			}
			position.y += UIEditorUtil.LINE_HEIGHT * 2;

			if( animationOrTriggerMode ) {
				var modeProp = property.FindPropertyRelative("Mode");
				EditorGUI.PropertyField(position, modeProp);

				var mode = modeProp.intValue;
				position.y += UIEditorUtil.LINE_HEIGHT;
				if( mode == 0 ) {
					var animatorProcessorProp = property.FindPropertyRelative("m_processor");
					EditorGUI.PropertyField(position, animatorProcessorProp);
				}
				else {
					var previewRect = position;
					previewRect.xMax = previewRect.x + 120;
					DrawPreview(property, previewRect);
					position.y += UIEditorUtil.LINE_HEIGHT;
					var animationProp = property.FindPropertyRelative(GetAnimationFieldName(mode));
					if( animationProp == null ) {
						Debug.LogError($"mode {GetAnimationFieldName(mode)} not exist");
						return;
					}
					animationModeActiveCount = DrawAnimationMode(position, animationProp, Mode);
					position.y += UIEditorUtil.LINE_HEIGHT;
					var originLabelWidth = EditorGUIUtility.labelWidth;
					for( var i = 0; i < Mode.Length; i++ ) {
						var type = Mode[i];
						var typProp = animationProp.FindPropertyRelative(type);
						var activeProp = typProp.FindPropertyRelative("m_active");
						if( !activeProp.boolValue )
							continue;
						position = CurrentAnimation[i].OnGUI(position, typProp);
						EditorGUIUtility.labelWidth = originLabelWidth;
					}
				}
			}
			else {
				
			}
		}
		
		private static Transform PreviewComponent(SerializedProperty property, out IUIAnimationPreview animation) {
			var previewComponent = property.serializedObject.targetObject as Behaviour;
			var field = previewComponent.GetType().GetField(property.name);
			animation = field.GetValue(previewComponent) as IUIAnimationPreview;
			return previewComponent.transform;
		}

		private static void StartPreview(SerializedProperty property) {
			var transform = PreviewComponent(property, out var animation);
			DOTweenEditorPreview.Stop();
			var previewGO = GameObject.Find("-[ DOTween Preview ► ]-");
			Object.DestroyImmediate(previewGO);
			
			animation.CacheStartValue(transform);
			var allTween = animation.CollectPreviewTween(transform);
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

					DOTweenEditorPreview.Stop();
					previewGO = GameObject.Find("-[ DOTween Preview ► ]-");
					Object.DestroyImmediate(previewGO);
					animation.Editor_Recovery(transform);
				};
			}

			DOTweenEditorPreview.Start();
			previewGO = GameObject.Find("-[ DOTween Preview ► ]-");
			previewGO.hideFlags |= HideFlags.HideAndDontSave;
		}

		private static void StopPreview(SerializedProperty property) {
			var transform = PreviewComponent(property, out var animation);
			DOTweenEditorPreview.Stop();
			var previewGO = GameObject.Find("-[ DOTween Preview ► ]-");
			Object.DestroyImmediate(previewGO);
			animation.Editor_Recovery(transform);
		}

		private static void DrawPreview(SerializedProperty property, Rect previewRect) {
			if( GUI.Button(previewRect, "Preview") ) {
				StartPreview(property);
			}

			previewRect.x = previewRect.xMax + 5;
			if( GUI.Button(previewRect, "Stop") ) {
				StopPreview(property);
			}
		}
	}
}