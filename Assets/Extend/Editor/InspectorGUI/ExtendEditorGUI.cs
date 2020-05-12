using System.Reflection;
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
				var target = property.GetPropertyParentObject();
				var hideAttr = property.GetAttribute<HideIfAttribute>();
				if( hideAttr != null ) {
					var fieldInfo = target.GetType().GetField(hideAttr.FieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					var val = fieldInfo.GetValue(target);
					if(val == hideAttr.Value)
						return;
				}
				
				var showAttr = property.GetAttribute<ShowIfAttribute>();
				if( showAttr != null ) {
					var fieldInfo = target.GetType().GetField(showAttr.FieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					var val = fieldInfo.GetValue(target);
					if(val != showAttr.Value)
						return;
				}
				
				var label = new GUIContent(property.GetLabel());
				EditorGUILayout.PropertyField(property, label);
			}
		}
	}
}