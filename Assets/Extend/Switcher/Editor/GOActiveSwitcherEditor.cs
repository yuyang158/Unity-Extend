using UnityEditor;
using UnityEngine;

namespace Extend.Switcher.Editor {
	[CustomPropertyDrawer(typeof(GOActiveSwitcher))]
	public class GOActiveSwitcherEditor : PropertyDrawer {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			position.height = EditorGUIUtility.singleLineHeight;
			var totalWidth = position.width;
			position.width = totalWidth * 0.7f;

			var goProp = property.FindPropertyRelative("GO");
			EditorGUI.ObjectField(position, goProp, new GUIContent("GameObject"));

			position.x += position.width;
			position.width = totalWidth * 0.3f;
			
			var activeProp = property.FindPropertyRelative("Active");
			activeProp.boolValue = EditorGUI.ToggleLeft(position, string.Empty, activeProp.boolValue);
		}
	}
}