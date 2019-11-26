using System;
using XLua;

namespace Extend.LuaBindingData {
	[Serializable]
	public class LuaBindingUOData : LuaBindingDataBase {
		public UnityEngine.Object Data;
		public override void ApplyToLuaInstance(LuaTable instance) {
			instance.Set(FieldName, Data);
		}
		
		
#if UNITY_EDITOR
		public override void OnPropertyDrawer(UnityEditor.SerializedProperty prop) {
			if( editorContent == null || string.IsNullOrEmpty(editorContent.text) ) {
				editorContent = new UnityEngine.GUIContent(FieldName);
			}
			Type type;
			if( FieldType.StartsWith("CS.") ) {
				var typeName = FieldType.Substring(3);
				type = String2TypeCache.GetType(typeName);
			}
			else {
				type = typeof(LuaBinding);
			}

			UnityEditor.EditorGUILayout.ObjectField(prop, type, editorContent);
		}
#endif
	}
}