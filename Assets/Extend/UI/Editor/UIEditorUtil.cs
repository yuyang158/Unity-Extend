using UnityEditor;
using UnityEngine;

namespace Extend.UI.Editor {
	public static class UIEditorUtil {
		public static readonly Color[] UI_ANIMATION_COLORS = {
			new Color(0, 0.5f, 0, 0.4f),
			new Color(0.45f, 0.4f, 0, 0.4f),
			new Color(0.4f, 0, 0, 0.4f),
			new Color(0.4f, 0, 0.4f, 0.4f)
		};
		
		public static readonly string[] PUNCH_ANIMATION_MODE = {"Move", "Rotate", "Scale"};
		public static readonly string[] STATE_ANIMATION_MODE = {"Move", "Rotate", "Scale", "Fade"};
		public const float ROW_CONTROL_MARGIN = 5;
		public static readonly float LINE_HEIGHT = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

		private static GUIStyle m_buttonSelectedStyle;
		public static GUIStyle ButtonSelectedStyle {
			get {
				if( m_buttonSelectedStyle == null ) {
					m_buttonSelectedStyle = new GUIStyle(GUI.skin.button) {
						normal = new GUIStyleState() {
							background = GUI.skin.button.onActive.scaledBackgrounds[0],
							textColor = new Color(0.8f, 0.8f, 0.8f, 1)
						}
					};
				}

				return m_buttonSelectedStyle;
			}
		}
	}
}