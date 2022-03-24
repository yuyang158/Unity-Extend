using System;
using Extend.Asset;
using XLua;

namespace Extend.LuaBindingData {
	[Serializable, UnityEngine.Scripting.Preserve]
	public class LuaBindingAssetReferenceData : LuaBindingDataBase {
		public Type AssetType { private get; set; }

		public AssetReference Data;

		public override void ApplyToLuaInstance(LuaTable instance) {
			instance.SetInPath(FieldName, Data);
		}

		public override void Destroy() {
			Data?.Dispose();
		}

#if UNITY_EDITOR
		public override void OnPropertyDrawer(UnityEditor.SerializedProperty prop, string displayName) {
			if( editorContent == null || string.IsNullOrEmpty(editorContent.text) ) {
				var name = UnityEditor.ObjectNames.NicifyVariableName(FieldName);
				editorContent = string.IsNullOrEmpty(displayName) ? new UnityEngine.GUIContent(name) : new UnityEngine.GUIContent(displayName);
			}

			editorContent.tooltip = AssetType == null ? string.Empty : AssetType.FullName;
			UnityEditor.EditorGUILayout.PropertyField(prop, editorContent);
		}
#endif
	}
}