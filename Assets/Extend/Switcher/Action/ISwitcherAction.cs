
using UnityEngine;

namespace Extend.Switcher.Action {
	public interface ISwitcherAction {
		void ActiveSwitcher();

#if UNITY_EDITOR
		void OnEditorGUI(Rect rect, UnityEditor.SerializedProperty property);
		
		float GetEditorHeight(UnityEditor.SerializedProperty property);
#endif
	}
}