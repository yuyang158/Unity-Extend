using Extend.Common;
using UnityEditor;
using UnityEngine;

namespace Extend.Editor.InspectorGUI {
	public static class ExtendEditorGUI {
		public static void PropertyField_Layout(SerializedProperty property, bool includeChildren) {
			var specialCastAttribute = property.GetAttribute<SpecialCaseAttribute>();
			if( specialCastAttribute != null ) {
				specialCastAttribute.GetDrawer().OnGUI(property);
			}
			else {
				var label = new GUIContent(property.GetLabel());
				EditorGUILayout.PropertyField(property, label);
			}
		}
	}
}