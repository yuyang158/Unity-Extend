using System;
using XLua;

namespace Extend.LuaBindingData {
	[Serializable]
	public abstract class LuaBindingDataBase {
		public string FieldName;
		[NonSerialized]
		public string FieldType;
		public abstract void ApplyToLuaInstance(LuaTable instance);

#if UNITY_EDITOR
		protected UnityEngine.GUIContent editorContent;
		public virtual void OnPropertyDrawer(UnityEditor.SerializedProperty prop) {
			if( editorContent == null || string.IsNullOrEmpty(editorContent.text) ) {
				editorContent = new UnityEngine.GUIContent(FieldName);
			}
			UnityEditor.EditorGUILayout.PropertyField(prop, editorContent);
		}
#endif
	}
}