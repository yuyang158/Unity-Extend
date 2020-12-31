using System;
using Extend.Common;
using UnityEngine;

namespace Extend.Switcher.Action {
	[Serializable]
	public class AnimatorSwitcherAction : ISwitcherAction {
		public AnimatorParamProcessor Processor;

		public void ActiveSwitcher() {
			Processor.Apply();
		}

#if UNITY_EDITOR
		public void OnEditorGUI(Rect rect, UnityEditor.SerializedProperty property) {
			var processorProperty = property.FindPropertyRelative("Processor");
			UnityEditor.EditorGUI.PropertyField(rect, processorProperty);
		}

		public float GetEditorHeight(UnityEditor.SerializedProperty property) {
			var processorProperty = property.FindPropertyRelative("Processor");
			return UnityEditor.EditorGUI.GetPropertyHeight(processorProperty);
		}
#endif
	}
}