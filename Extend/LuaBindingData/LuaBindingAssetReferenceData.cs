using System;
using Extend.AssetService;
using XLua;

namespace Extend.LuaBindingData {
	[Serializable]
	public class LuaBindingAssetReferenceData : LuaBindingDataBase {
		public AssetReference Data;
		public override void ApplyToLuaInstance(LuaTable instance) {
			instance.SetInPath(FieldName, Data);
		}

#if UNITY_EDITOR
		public override void OnPropertyDrawer(UnityEditor.SerializedProperty prop) {
			if( editorContent == null || string.IsNullOrEmpty(editorContent.text) ) {
				var name = UnityEditor.ObjectNames.NicifyVariableName(FieldName);
				editorContent = new UnityEngine.GUIContent(name);
			}

			UnityEditor.EditorGUILayout.PropertyField(prop, editorContent);
		}
#endif
	}
}