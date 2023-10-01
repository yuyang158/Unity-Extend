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
			m_drawers.Add(typeof(AnimatorSwitcherAction), new DefaultActionDrawer());
			m_drawers.Add(typeof(GOActiveSwitcherAction), new GOActiveSwitcherActionDrawer());
			m_drawers.Add(typeof(TextAssignSwitcherAction), new DefaultActionDrawer());
			m_drawers.Add(typeof(UIMaterialSwitcherAction), new DefaultActionDrawer());
			m_drawers.Add(typeof(ImageSwitcherAction), new DefaultActionDrawer());
			m_drawers.Add(typeof(TweenAnimationAction), new DefaultActionDrawer());
		}

		public static ActionDrawer GetDrawer(Type type) {
			return m_drawers[type];
		}
	}

	public class DefaultActionDrawer : ActionDrawer {
		private float m_height;
		public override void OnEditorGUI(Rect rect, SerializedProperty property) {
			m_height = 0;
			int depth = property.depth;
			foreach( SerializedProperty subProp in property ) {
				if( subProp.name == "m_fold" ) {
					continue;
				}
				if( subProp.depth != depth + 1 ) {
					continue;
				}
				EditorGUI.PropertyField(rect, subProp);
				var height = EditorGUI.GetPropertyHeight(subProp);
				rect.y += height;
				m_height += height;
			}
		}

		public override float GetEditorHeight(SerializedProperty property) {
			return m_height;
		}
	}

	public class GOActiveSwitcherActionDrawer : ActionDrawer {
		private static readonly GUIContent m_gameObjectTitle = new GUIContent("Game Object Active");
		public override void OnEditorGUI(Rect rect, SerializedProperty property) {
			var goProp = property.FindPropertyRelative("m_go");
			rect.xMax -= 25;
			rect.height = EditorGUIUtility.singleLineHeight;
			EditorGUI.PropertyField(rect, goProp, m_gameObjectTitle);

			var activeProp = property.FindPropertyRelative("m_active");
			var toggleRect = rect;
			toggleRect.xMin = rect.xMax;
			toggleRect.xMax += 20;
			activeProp.boolValue = EditorGUI.ToggleLeft(toggleRect, GUIContent.none, activeProp.boolValue);
		}

		public override float GetEditorHeight(SerializedProperty property) {
			return UIEditorUtil.LINE_HEIGHT;
		}
	}
}