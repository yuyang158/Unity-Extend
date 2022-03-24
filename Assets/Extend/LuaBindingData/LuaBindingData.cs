using System;
using UnityEngine.Scripting;
using XLua;

namespace Extend.LuaBindingData {
	[Serializable, Preserve]
	public abstract class LuaBindingDataBase {
		public string FieldName;
		[NonSerialized]
		public string FieldType;
		public abstract void ApplyToLuaInstance(LuaTable instance);

		public virtual void Destroy() {
			
		}

#if UNITY_EDITOR
		protected UnityEngine.GUIContent editorContent;
		public virtual void OnPropertyDrawer(UnityEditor.SerializedProperty prop, string displayName) {
			if( editorContent == null || string.IsNullOrEmpty(editorContent.text) ) {
				var name = UnityEditor.ObjectNames.NicifyVariableName(FieldName);
				editorContent = string.IsNullOrEmpty(displayName) ? new UnityEngine.GUIContent(name) : new UnityEngine.GUIContent(displayName);
			}
			
			UnityEditor.EditorGUILayout.PropertyField(prop, editorContent);
		}
#endif
	}
}