using Extend.Common.Editor;
using Extend.Editor;
using UnityEditor;
using UnityEngine;

namespace Extend.LuaBindingEvent.Editor {
	[CustomPropertyDrawer(typeof(LuaEmmyFunction))]
	public class LuaEmmyFunctionPropertyDrawer : PropertyDrawer {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			var totalWidth = position.width;
			position.width = totalWidth * .3f;
			position.height = EditorGUIUtility.singleLineHeight;
			var bindingProp = property.FindPropertyRelative("Binding");
			var globalProp = property.FindPropertyRelative("GlobalMethod");

			var xMax = position.xMax;
			position.xMax = position.xMin + UIEditorUtil.LINE_HEIGHT;
			globalProp.boolValue = EditorGUI.ToggleLeft(position, GUIContent.none, globalProp.boolValue);
			position.xMin = position.xMax + UIEditorUtil.ROW_CONTROL_MARGIN;
			position.xMax = xMax;
			EditorGUI.ObjectField(position, bindingProp, GUIContent.none);
			if( bindingProp.objectReferenceValue == null && !globalProp.boolValue ) {
				return;
			}

			if( bindingProp.objectReferenceValue != null ) {
				globalProp.boolValue = false;
			}

			position.x += position.width + UIEditorUtil.ROW_CONTROL_MARGIN;
			position.width = totalWidth * .7f - UIEditorUtil.ROW_CONTROL_MARGIN;
			var binding = bindingProp.objectReferenceValue as LuaBinding;
			var methodNameProp = property.FindPropertyRelative("LuaMethodName");
			if( binding ) {
				var descriptor = LuaClassEditorFactory.GetDescriptor(binding.LuaFile);
				if( descriptor == null )
					return;
				var selected = descriptor.Methods.IndexOf(methodNameProp.stringValue);
				var newIndex = EditorGUI.Popup(position, selected, descriptor.Methods.ToArray());
				if( newIndex != selected ) {
					methodNameProp.stringValue = descriptor.Methods[newIndex];
				}
			}
			else {
				methodNameProp.stringValue = EditorGUI.TextField(position, GUIContent.none, methodNameProp.stringValue);
			}
		}
	}
}
