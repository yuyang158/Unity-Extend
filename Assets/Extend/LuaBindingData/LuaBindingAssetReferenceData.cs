using System;
using System.Reflection;
using Extend.AssetService;
using XLua;

namespace Extend.LuaBindingData {
	[Serializable]
	public class LuaBindingAssetReferenceData : LuaBindingDataBase {
		public Type AssetType { private get; set; }

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

			editorContent.tooltip = AssetType == null ? string.Empty : AssetType.FullName;
			UnityEditor.EditorGUILayout.PropertyField(prop, editorContent);
		}
#endif
	}
}