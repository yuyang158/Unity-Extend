using System;
using Extend.Common;
using Extend.Common.Editor;
using UnityEditor;
using UnityEngine;

namespace Extend.Editor {
	[CustomPropertyDrawer(typeof(LuaFileAttribute))]
	public class LuaFilePropertyDrawer : PropertyDrawer {
		private bool m_foldoutDetail;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			var descriptor = LuaClassEditorFactory.GetDescriptor(property.stringValue);
			if( descriptor == null || descriptor.Methods.Count == 0 ) {
				return UIEditorUtil.LINE_HEIGHT;
			}

			if( !m_foldoutDetail ) {
				return UIEditorUtil.LINE_HEIGHT * 2;
			}

			var height = descriptor.Fields.Count * UIEditorUtil.LINE_HEIGHT + descriptor.Methods.Count * UIEditorUtil.LINE_HEIGHT;
			return UIEditorUtil.LINE_HEIGHT * 2 + height;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			position.height = EditorGUIUtility.singleLineHeight;
			property.stringValue = EditorGUI.TextField(position, label, property.stringValue);

			var descriptor = LuaClassEditorFactory.GetDescriptor(property.stringValue);
			if( descriptor == null || descriptor.Methods.Count == 0 ) {
				return;
			}
			position.y += UIEditorUtil.LINE_HEIGHT;
			m_foldoutDetail = EditorGUI.Foldout(position, m_foldoutDetail, $"{descriptor.ClassName} Methods");
			if( !m_foldoutDetail )
				return;

			EditorGUI.indentLevel += 1;
			try {
				foreach( var method in descriptor.Methods ) {
					position.y += UIEditorUtil.LINE_HEIGHT;
					EditorGUI.LabelField(position, $"Method : {method}");
				}
			}
			catch( Exception e ) {
				Debug.LogException(e);
			}
			finally {
				EditorGUI.indentLevel -= 1;
			}
		}
	}
}