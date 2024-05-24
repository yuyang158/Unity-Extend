using System;
using Extend.Common;
using UnityEngine;
using XLua;
using Object = UnityEngine.Object;

namespace Extend.LuaBindingData {
	[Serializable, UnityEngine.Scripting.Preserve]
	public class LuaBindingUOData : LuaBindingDataBase {
		public Object Data;

		public override void ApplyToLuaInstance(LuaTable instance) {
			if( !Data ) {
				// Debug.LogWarning($"Field {FieldName} value is null!");
				return;
			}
			instance.Set(FieldName, Data);
		}

#if UNITY_EDITOR
		public override void OnPropertyDrawer(UnityEditor.SerializedProperty prop, string displayName) {
			if( editorContent == null || string.IsNullOrEmpty(editorContent.text) ) {
				var name = UnityEditor.ObjectNames.NicifyVariableName(FieldName);
				editorContent = string.IsNullOrEmpty(displayName) ? new GUIContent(name) : new GUIContent(displayName);
			}

			Type type;
			if( FieldType.StartsWith("CS.") ) {
				var typeName = FieldType.Substring(3);
				type = String2TypeCache.GetType(typeName);
			}
			else {
				type = typeof(LuaBinding);
			}
			UnityEditor.EditorGUI.BeginChangeCheck();
			UnityEditor.EditorGUILayout.ObjectField(prop, type, editorContent);
			if( UnityEditor.EditorGUI.EndChangeCheck() ) {
				var path = UnityEditor.AssetDatabase.GetAssetPath(prop.objectReferenceValue);
				if(string.IsNullOrEmpty(path))
					return;

				if( path.EndsWith(".prefab") ) {
					UnityEditor.EditorUtility.DisplayDialog("ERROR", $"{FieldName} should use AssetReference to a non scene object!", "OK");
					prop.objectReferenceValue = null;
				}
			}
		}
#endif
	}
}
