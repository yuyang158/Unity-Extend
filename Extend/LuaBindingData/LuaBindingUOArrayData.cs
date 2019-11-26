using System;
using XLua;

namespace Extend.LuaBindingData {
	[Serializable]
	public class LuaBindingUOArrayData : LuaBindingDataBase {
		public UnityEngine.Object[] Data;
		public override void ApplyToLuaInstance(LuaTable instance) {
			var arrayLuaTable = LuaVM.Default.NewTable();
			for( var i = 0; i < Data.Length; i++ ) {
				arrayLuaTable.Set(i + 1, Data[i]);
			}
			instance.Set(FieldName, arrayLuaTable);
		}
		
		
#if UNITY_EDITOR
		private UnityEditorInternal.ReorderableList reList;
		public override void OnPropertyDrawer(UnityEditor.SerializedProperty prop) {
			if( editorContent == null || string.IsNullOrEmpty(editorContent.text) ) {
				editorContent = new UnityEngine.GUIContent(FieldName);
			}

			if( reList == null ) {
				Type type;
				if( FieldType.StartsWith("CS.") ) {
					var typeName = FieldType.Substring(3);
					type = String2TypeCache.GetType(typeName);
				}
				else {
					type = typeof(LuaBinding);
				}
				reList = new UnityEditorInternal.ReorderableList(prop.serializedObject, prop);
				reList.elementHeight = UnityEditor.EditorGUIUtility.singleLineHeight;
				reList.drawHeaderCallback += rect => {
					UnityEditor.EditorGUI.LabelField(rect, FieldName);
				};
				reList.drawElementCallback += (rect, index, active, focused) => {
					var elemProp = reList.serializedProperty.GetArrayElementAtIndex(index);
					UnityEditor.EditorGUI.ObjectField(rect, elemProp, type);
				};
			}

			reList.serializedProperty = prop;
			reList.DoLayoutList();
		}
#endif
	}
}