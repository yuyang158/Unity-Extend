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
				var name = UnityEditor.ObjectNames.NicifyVariableName(FieldName);
				editorContent = new UnityEngine.GUIContent(name);
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

				reList = new UnityEditorInternal.ReorderableList(prop.serializedObject, prop) {
					elementHeight = UnityEditor.EditorGUIUtility.singleLineHeight
				};
				reList.drawHeaderCallback += rect => {
					UnityEditor.EditorGUI.LabelField(rect, FieldName);
				};
				reList.drawElementCallback += (rect, index, active, focused) => {
					var elemProp = reList.serializedProperty.GetArrayElementAtIndex(index);
					UnityEditor.EditorGUI.BeginChangeCheck();
					UnityEditor.EditorGUI.ObjectField(rect, elemProp, type, editorContent);
					if( UnityEditor.EditorGUI.EndChangeCheck() ) {
						var path = UnityEditor.AssetDatabase.GetAssetPath(elemProp.objectReferenceValue);
						if(string.IsNullOrEmpty(path))
							return;

						UnityEditor.EditorUtility.DisplayDialog("ERROR", $"{FieldName} should use AssetReference to a non scene object!", "OK");
						elemProp.objectReferenceValue = null;
					}
				};
			}

			reList.serializedProperty = prop;
			reList.DoLayoutList();
		}
#endif
	}
}