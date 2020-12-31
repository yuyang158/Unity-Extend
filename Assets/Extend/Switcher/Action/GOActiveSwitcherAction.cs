using System;
using UnityEngine;

namespace Extend.Switcher.Action {
	[Serializable]
	public class GOActiveSwitcherAction : ISwitcherAction {
		public GameObject GO;
		public bool Active;
		
		public void ActiveSwitcher() {
			GO.SetActive(Active);
		}
		
		
#if UNITY_EDITOR
		public void OnEditorGUI(Rect rect, UnityEditor.SerializedProperty property) {
			var goProp = property.FindPropertyRelative("GO");
			rect.xMax -= 20;
			UnityEditor.EditorGUI.PropertyField(rect, goProp);

			var activeProp = property.FindPropertyRelative("Active");
			var toggleRect = rect;
			toggleRect.xMin = rect.xMax;
			toggleRect.xMax += 20;
			activeProp.boolValue = UnityEditor.EditorGUI.Toggle(toggleRect, activeProp.boolValue);
		}

		public float GetEditorHeight(UnityEditor.SerializedProperty property) {
			return Common.Editor.UIEditorUtil.LINE_HEIGHT;
		}
#endif
	}
}