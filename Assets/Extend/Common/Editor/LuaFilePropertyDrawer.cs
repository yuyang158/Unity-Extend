using UnityEditor;
using UnityEngine;

namespace Extend.Common.Editor {
	[CustomPropertyDrawer(typeof(LuaFileAttribute))]
	public class LuaFilePropertyDrawer : PropertyDrawer {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			property.stringValue = EditorGUI.TextField(position, label, property.stringValue);
		}
	}
}