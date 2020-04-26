using Extend.Asset.Attribute;
using Extend.Common;
using UnityEditor;
using UnityEngine;

namespace Extend.Asset.Editor {
	[CustomPropertyDrawer(typeof(AssetReference), true)]
	public class AssetReferencePropertyDrawer : PropertyDrawer {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			if( !string.IsNullOrEmpty(label.text) ) {
				if( !label.text.Contains("(Asset)") )
					label.text += " (Asset)";
			}
			
			var guidProp = property.FindPropertyRelative("assetGUID");
			var attributes = fieldInfo.GetCustomAttributes(typeof(AssetReferenceAssetTypeAttribute), false);
			var type = typeof(Object);
			if( !string.IsNullOrEmpty(label.tooltip) ) {
				type = String2TypeCache.GetType(label.tooltip);
			}
			else {
				if( attributes.Length > 0 ) {
					var attr = attributes[0] as AssetReferenceAssetTypeAttribute;
					type = attr.AssetType;
				}
			}
			var assetPath = AssetDatabase.GUIDToAssetPath(guidProp.stringValue);
			var resObj = AssetDatabase.LoadAssetAtPath(assetPath, type);
			var newResObj = EditorGUI.ObjectField(position, label, resObj, type, false);
			if( newResObj != resObj ) {
				if( newResObj == null ) {
					guidProp.stringValue = null;
				}
				else {
					assetPath = AssetDatabase.GetAssetPath(newResObj);
					var path = assetPath.ToLower();
					if( path.StartsWith("assets/resources") ) {
						guidProp.stringValue = AssetDatabase.AssetPathToGUID(assetPath);
					}
					else {
						var settings = AssetDatabase.LoadAssetAtPath<StaticABSettings>(StaticAssetBundleWindow.SETTING_FILE_PATH);
						if( settings.ContainExtraObject(newResObj) ) {
							guidProp.stringValue = AssetDatabase.AssetPathToGUID(assetPath);
							return;
						}

						EditorUtility.DisplayDialog("ERROR", $"无法在配置中找到资源:{newResObj}", "OK");
					}
				}
			}
		}
	}
}