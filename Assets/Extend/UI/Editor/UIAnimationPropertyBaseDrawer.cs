using UnityEditor;
using UnityEngine;

namespace Extend.UI.Editor {
	public abstract class UIAnimationPropertyBaseDrawer : PropertyDrawer {
		protected SerializedProperty animationProperty;
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			animationProperty = property;
			var enabledProp = property.FindPropertyRelative("m_enabled");
			if( !enabledProp.boolValue )
				return UIEditorUtil.LINE_HEIGHT;

			var modeProp = property.FindPropertyRelative("Mode");
			var mode = modeProp.intValue;
			if( mode == 0 ) {
				return UIEditorUtil.LINE_HEIGHT * 5;
			}
			return UIEditorUtil.LINE_HEIGHT * 3 + ( SingleDoTweenHeight + EditorGUIUtility.standardVerticalSpacing ) * animationModeActiveCount + UIEditorUtil.LINE_HEIGHT;
		}

		private int animationModeActiveCount;
		protected abstract UIAnimationParamCombine[] CurrentAnimation { get; }
		protected abstract float SingleDoTweenHeight { get; }
		protected abstract string[] Mode { get; }

		protected virtual string GetAnimationFieldName(int mode) {
			return "m_state";
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
				animationModeActiveCount = UIEditorUtil.DrawAnimationMode(position, animationProp, Mode);
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