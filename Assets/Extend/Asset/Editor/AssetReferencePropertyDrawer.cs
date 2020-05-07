using System.Reflection;
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

			position.xMax -= EditorGUIUtility.singleLineHeight;
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

			position.xMax += EditorGUIUtility.singleLineHeight;
			position.xMin = position.xMax - EditorGUIUtility.singleLineHeight;
			GUI.enabled = newResObj != null;
			if( GUI.Button(position, LazyTextureLoader.QuickEditPen, GUI.skin.box) && newResObj != null ) {
				var originObj = Selection.objects;
				// Retrieve the existing Inspector tab, or create a new one if none is open
				var inspectorWindow = EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow"));
				// Get the size of the currently window
				var size = new Vector2(inspectorWindow.position.width, inspectorWindow.position.height);
				// Clone the inspector tab (optionnal step)
				inspectorWindow = Object.Instantiate(inspectorWindow);
				// Set min size, and focus the window
				inspectorWindow.minSize = size;
				inspectorWindow.Show();
				inspectorWindow.Focus();
				Selection.activeObject = newResObj;

				inspectorWindow.GetType()
					.GetProperty("isLocked",
						BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
					.GetSetMethod().Invoke(inspectorWindow, new[] {
						(object)true
					});

				Selection.objects = originObj;
			}
			GUI.enabled = true;
		}
	}
}