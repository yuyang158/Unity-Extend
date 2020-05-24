using System.IO;
using Extend.Common;
using Extend.Common.Editor;
using UnityEditor;
using UnityEngine;

namespace Extend.Editor {
	[CustomPropertyDrawer(typeof(AssetPathAttribute))]
	public class AssetPathPropertyDrawer : PropertyDrawer {
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return UIEditorUtil.LINE_HEIGHT;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			var pathAttr = attribute as AssetPathAttribute;
			var path = $"{pathAttr.RootDir}/{property.stringValue}{pathAttr.Extension}";
			var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
			var newAsset = EditorGUILayout.ObjectField(property.displayName, asset, pathAttr.AssetType, false);
			if( newAsset == null ) {
				property.stringValue = "";
			}
			else if( newAsset != asset ) {
				var newPath = AssetDatabase.GetAssetPath(newAsset);
				if( !newPath.StartsWith(pathAttr.RootDir) || Path.GetExtension(newPath) != pathAttr.Extension ) {
					property.stringValue = "";
					return;
				}
				
				property.stringValue = newPath.Substring(pathAttr.RootDir.Length + 1, 
					newPath.Length - pathAttr.RootDir.Length - pathAttr.Extension.Length - 1);
			}
		}
	}
}