using System;
using XLua;

namespace Extend.LuaBindingData {
	[Serializable]
	public abstract class LuaBindingDataBase {
		public string FieldName;
		[NonSerialized]
		public string FieldType;
		public abstract void ApplyToLuaInstance(LuaTable instance);

		public virtual void Destroy() {
			
		}

#if UNITY_EDITOR
		protected UnityEngine.GUIContent editorContent;
		public virtual void OnPropertyDrawer(UnityEditor.SerializedProperty prop) {
			if( editorContent == null || string.IsNullOrEmpty(editorContent.text) ) {
				var name = UnityEditor.ObjectNames.NicifyVariableName(FieldName);
				editorContent = new UnityEngine.GUIContent(name);
			}
			
			UnityEditor.EditorGUILayout.PropertyField(prop, editorContent);
		}
#endif
	}
}