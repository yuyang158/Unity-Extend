using System.Collections.Generic;
using System.Linq;
using DG.DOTweenEditor;
using DG.Tweening;
using UnityEditor;
using UnityEngine;

namespace Extend.UI.Editor {
	public static class UIEditorUtil {
		public static readonly Color[] TransformModeColors = {
			new Color(0, 0.5f, 0, 0.4f),
			new Color(0.45f, 0.4f, 0, 0.4f),
			new Color(0.4f, 0, 0, 0.4f),
			new Color(0.4f, 0, 0.4f, 0.4f)
		};

		public static void StartPreview(SerializedProperty property) {
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

		public static void StopPreview(SerializedProperty property) {
			var transform = PreviewComponent(property, out var animation);
			DOTweenEditorPreview.Stop();
			animation.Editor_Recovery(transform);
			var previewGO = GameObject.Find("-[ DOTween Preview ► ]-");
			Object.DestroyImmediate(previewGO);
		}

		public static int DrawAnimationMode(Rect transformModeSelectionRect, SerializedProperty animationProp, IReadOnlyCollection<string> types) {
			var animationModeActiveCount = 0;
			transformModeSelectionRect.width /= types.Count;
			foreach( var type in types ) {
				var typProp = animationProp.FindPropertyRelative(type);
				var activeProp = typProp.FindPropertyRelative("active");

				activeProp.boolValue = EditorGUI.ToggleLeft(transformModeSelectionRect, type, activeProp.boolValue);
				transformModeSelectionRect.x = transformModeSelectionRect.xMax;
				if( activeProp.boolValue )
					animationModeActiveCount++;
			}

			return animationModeActiveCount;
		}
		
		private static Transform PreviewComponent(SerializedProperty property, out IUIAnimationPreview animation) {
			var previewComponent = property.serializedObject.targetObject as Behaviour;
			var field = previewComponent.GetType().GetField(property.name);
			animation = field.GetValue(previewComponent) as IUIAnimationPreview;
			return previewComponent.transform;
		}
	}
}