using System;
using Extend.Common;
using XLua;

namespace Extend.LuaBindingData {
	[Serializable, UnityEngine.Scripting.Preserve]
	public class LuaBindingUOArrayData : LuaBindingDataBase {
		public UnityEngine.Object[] Data;

		public override void ApplyToLuaInstance(LuaTable instance) {
			var luaVM = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			var arrayLuaTable = luaVM.NewTable();
			for( var i = 0; i < Data.Length; i++ ) {
				arrayLuaTable.Set(i + 1, Data[i]);
			}

			instance.Set(FieldName, arrayLuaTable);
		}


#if UNITY_EDITOR
		private UnityEditorInternal.ReorderableList reList;
		public override void OnPropertyDrawer(UnityEditor.SerializedProperty prop, string displayName) {
			if( editorContent == null || string.IsNullOrEmpty(editorContent.text) ) {
				var name = UnityEditor.ObjectNames.NicifyVariableName(FieldName);
				editorContent = string.IsNullOrEmpty(displayName) ? new UnityEngine.GUIContent(name) : new UnityEngine.GUIContent(displayName);
			}

			if( reList == null ) {
				Type type;
				if( FieldType.StartsWith("CS.") ) {
					var typeName = FieldType.Substring(3, FieldType.Length - 5);
					type = String2TypeCache.GetType(typeName);
				}
				else {
					type = typeof(LuaBinding);
				}

				reList = new UnityEditorInternal.ReorderableList(prop.serializedObject, prop) {
					elementHeight = UnityEditor.EditorGUIUtility.singleLineHeight
				};
				reList.drawHeaderCallback += rect => {
					UnityEditor.EditorGUI.LabelField(rect, UnityEditor.ObjectNames.NicifyVariableName(FieldName));
				};
				reList.drawElementCallback += (rect, index, active, focused) => {
					var elemProp = reList.serializedProperty.GetArrayElementAtIndex(index);
					UnityEditor.EditorGUI.BeginChangeCheck();
					UnityEditor.EditorGUI.ObjectField(rect, elemProp, type, editorContent);
					if( UnityEditor.EditorGUI.EndChangeCheck() ) {
						var path = UnityEditor.AssetDatabase.GetAssetPath(elemProp.objectReferenceValue);
						if(string.IsNullOrEmpty(path))
							return;

						if( path.EndsWith(".prefab") ) {
							UnityEditor.EditorUtility.DisplayDialog("ERROR", $"{FieldName} should use AssetReference to a non scene object!", "OK");
							elemProp.objectReferenceValue = null;
						}
					}
				};
			}

			reList.serializedProperty = prop;
			reList.DoLayoutList();
		}
#endif
	}
}
