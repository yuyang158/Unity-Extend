using UnityEditor;
using UnityEngine;

namespace Extend.UI.Editor {
	public interface IUIAnimationDrawer {
		void OnGUI(Rect position, SerializedProperty property, GUIContent label);
	}
}