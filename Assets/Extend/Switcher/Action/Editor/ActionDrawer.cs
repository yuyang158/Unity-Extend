using System;
using System.Collections.Generic;
using Extend.Common.Editor;
using UnityEditor;
using UnityEngine;

namespace Extend.Switcher.Action.Editor {
	public abstract class ActionDrawer {
		public abstract void OnEditorGUI(Rect rect, SerializedProperty property);
		public abstract float GetEditorHeight(SerializedProperty property);

		private static readonly Dictionary<Type, ActionDrawer> m_drawers = new Dictionary<Type, ActionDrawer>();

		static ActionDrawer() {
			m_drawers.Add(typeof(AnimatorSwitcherAction), new AnimatorSwitcherActionDrawer());
			m_drawers.Add(typeof(GOActiveSwitcherAction), new GOActiveSwitcherActionDrawer());
			m_drawers.Add(typeof(TextAssignSwitcherAction), new TextAssignSwitcherActionDrawer());
			m_drawers.Add(typeof(GraphicMaterialSwitcherAction), new GraphicMaterialSwitcherActionDrawer());
			m_drawers.Add(typeof(ImageSwitcherAction), new ImageSwitcherActionDrawer());
		}

		public static ActionDrawer GetDrawer(Type type) {
			return m_drawers[type];
		}
	}

	public class AnimatorSwitcherActionDrawer : ActionDrawer {
		public override void OnEditorGUI(Rect rect, SerializedProperty property) {
			var processorProperty = property.FindPropertyRelative("m_processor");
			EditorGUI.PropertyField(rect, processorProperty);
		}

		public override float GetEditorHeight(SerializedProperty property) {
			var processorProperty = property.FindPropertyRelative("m_processor");
			return EditorGUI.GetPropertyHeight(processorProperty);
		}
	}

	public class GOActiveSwitcherActionDrawer : ActionDrawer {
		private static readonly GUIContent m_gameObjectTitle = new GUIContent("Game Object Active");
		public override void OnEditorGUI(Rect rect, SerializedProperty property) {
			var goProp = property.FindPropertyRelative("m_go");
			rect.xMax -= 20;
			rect.height = EditorGUIUtility.singleLineHeight;
			EditorGUI.PropertyField(rect, goProp, m_gameObjectTitle);

			var activeProp = property.FindPropertyRelative("m_active");
			var toggleRect = rect;
			toggleRect.xMin = rect.xMax;
			toggleRect.xMax += 20;
			activeProp.boolValue = EditorGUI.Toggle(toggleRect, activeProp.boolValue);
		}

		public override float GetEditorHeight(SerializedProperty property) {
			return UIEditorUtil.LINE_HEIGHT;
		}
	}

	public class TextAssignSwitcherActionDrawer : ActionDrawer {
		public override void OnEditorGUI(Rect rect, SerializedProperty property) {
			rect.height = EditorGUIUtility.singleLineHeight;
			var textGUIProperty = property.FindPropertyRelative("m_textGUI");
			EditorGUI.PropertyField(rect, textGUIProperty);

			rect.y += UIEditorUtil.LINE_HEIGHT;
			var textProp = property.FindPropertyRelative("m_text");
			rect.height = UIEditorUtil.LINE_HEIGHT * 3 - EditorGUIUtility.standardVerticalSpacing;
			textProp.stringValue = EditorGUI.TextField(rect, textProp.stringValue);
		}

		public override float GetEditorHeight(SerializedProperty property) {
			return UIEditorUtil.LINE_HEIGHT * 4;
		}
	}

	public class GraphicMaterialSwitcherActionDrawer : ActionDrawer {
		public override void OnEditorGUI(Rect rect, SerializedProperty property) {
			rect.height = EditorGUIUtility.singleLineHeight;
			var graphicProp = property.FindPropertyRelative("m_graphic");
			EditorGUI.PropertyField(rect, graphicProp);
			rect.y += UIEditorUtil.LINE_HEIGHT;
			var materialProp = property.FindPropertyRelative("m_material");
			EditorGUI.PropertyField(rect, materialProp);
		}

		public override float GetEditorHeight(SerializedProperty property) {
			return UIEditorUtil.LINE_HEIGHT * 2;
		}
	}

	public class ImageSwitcherActionDrawer : ActionDrawer {
		public override void OnEditorGUI(Rect rect, SerializedProperty property) {
			rect.height = EditorGUIUtility.singleLineHeight;
			var imgProp = property.FindPropertyRelative("m_image");
			EditorGUI.PropertyField(rect, imgProp);
			rect.y += UIEditorUtil.LINE_HEIGHT;
			var spriteProp = property.FindPropertyRelative("m_sprite");
			EditorGUI.PropertyField(rect, spriteProp);
		}

		public override float GetEditorHeight(SerializedProperty property) {
			return UIEditorUtil.LINE_HEIGHT * 2;
		}
	}
}