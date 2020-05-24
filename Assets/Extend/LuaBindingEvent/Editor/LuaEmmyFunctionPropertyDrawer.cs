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
			EditorGUI.ObjectField(position, bindingProp, GUIContent.none);
			if( bindingProp.objectReferenceValue == null ) {
				return;
			}

			var binding = bindingProp.objectReferenceValue as LuaBinding;
			var descriptor = LuaClassEditorFactory.GetDescriptor(binding.LuaFile);
			if( descriptor == null )
				return;

			position.x += position.width + UIEditorUtil.ROW_CONTROL_MARGIN;
			position.width = totalWidth * .7f - UIEditorUtil.ROW_CONTROL_MARGIN;
			var methodNameProp = property.FindPropertyRelative("LuaMethodName");
			var selected = descriptor.Methods.IndexOf(methodNameProp.stringValue);
			var newIndex = EditorGUI.Popup(position, selected, descriptor.Methods.ToArray());
			if( newIndex != selected ) {
				methodNameProp.stringValue = descriptor.Methods[newIndex];
			}
		}
	}
}