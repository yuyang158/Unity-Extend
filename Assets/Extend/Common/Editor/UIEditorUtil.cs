using UnityEditor;
using UnityEngine;

namespace Extend.Common.Editor {
	public static class UIEditorUtil {
		public static readonly Color[] UI_ANIMATION_COLORS = {
			new Color(0, 0.5f, 0, 0.4f),
			new Color(0.45f, 0.4f, 0, 0.4f),
			new Color(0.4f, 0, 0, 0.4f),
			new Color(0.4f, 0, 0.4f, 0.4f)
		};
		
		public static readonly string[] PUNCH_ANIMATION_MODE = {"Move", "Rotate", "Scale"};
		public static readonly string[] STATE_ANIMATION_MODE = {"Move", "Rotate", "Scale", "Fade"};
		public static readonly string[] FLY_ANIMATION_MODE = {"MovePath", "Scale", "Fade"};
		public const float ROW_CONTROL_MARGIN = 5;
		public static readonly float LINE_HEIGHT = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

		public static Rect CalcMultiColumnRect(Rect position, int index, int totalColumn) {
			var rect = position;
			rect.width /= totalColumn;
			rect.x += rect.width * index;
			return rect;
		}

		private static GUIStyle m_buttonSelectedStyle;
		public static GUIStyle ButtonSelectedStyle {
			get {
				if( m_buttonSelectedStyle == null ) {
					var style = (GUIStyle)"ButtonMid";
					m_buttonSelectedStyle = new GUIStyle(style) {
						normal = new GUIStyleState() {
							background = style.onActive.scaledBackgrounds[0],
							textColor = new Color(0.8f, 0.8f, 0.8f, 1)
						}
					};
				}

				return m_buttonSelectedStyle;
			}
		}

		public static string RecursiveNodePath(Transform node) {
			var path = node.name;
			var parent = node.parent;
			while( parent ) {
				path = parent.name + path;
				parent = parent.parent;
			}

			return path;
		}
	}
}