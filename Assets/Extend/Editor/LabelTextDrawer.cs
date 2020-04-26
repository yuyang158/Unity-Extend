using Extend.Common;
using UnityEditor;
using UnityEngine;

namespace Extend.Editor {
	[CustomPropertyDrawer(typeof(LabelText))]
	public class LabelTextDrawer : PropertyDrawer {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			var labelText = attribute as LabelText;
			label.text = labelText.Text;
			EditorGUI.PropertyField(position, property, label);
		}
	}
}