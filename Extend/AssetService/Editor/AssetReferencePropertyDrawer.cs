using UnityEditor;
using UnityEngine;

namespace Extend.AssetService.Editor {
	[CustomPropertyDrawer(typeof(AssetReference))]
	public class AssetReferencePropertyDrawer : PropertyDrawer {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			var objProp = property.FindPropertyRelative("unityObject");
			EditorGUI.BeginChangeCheck();
			EditorGUI.PropertyField(position, objProp, new GUIContent(property.displayName));
			var dirty = EditorGUI.EndChangeCheck();
			if(!dirty)
				return;

			var pathProp = property.FindPropertyRelative("assetPath");
			if( objProp.objectReferenceValue == null ) {
				pathProp.stringValue = string.Empty;
			}
			else {
				var path = AssetDatabase.GetAssetPath(objProp.objectReferenceValue);
				if( string.IsNullOrEmpty(path) ) {
					objProp.objectReferenceValue = null;
				}
				else {
					path = path.ToLower();
					if( path.StartsWith("assets/resources") ) {
						pathProp.stringValue = path;
					}
					else {
						var settings = AssetDatabase.LoadAssetAtPath<StaticABSettings>(StaticAssetBundleWindow.SETTING_FILE_PATH);
						if( !settings.ContainExtraObject(objProp.objectReferenceValue) ) {
							EditorUtility.DisplayDialog("ERROR", $"无法在配置中找到资源:{objProp.objectReferenceValue}", "OK");
							objProp.objectReferenceValue = null;
						}
					}
				}
			}
		}
	}
}