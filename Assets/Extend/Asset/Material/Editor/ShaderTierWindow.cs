using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Extend.Asset.Material.Editor {
	public class ShaderTierWindow : EditorWindow {
		private ReorderableList m_shaderList;
		private const string ASSET_PATH = "Assets/Extend/Asset/Material/Editor/ShaderTiers.asset";

		[MenuItem("Tools/Shader Lod Tier")]
		private static void ShowWindow() {
			var win = CreateWindow<ShaderTierWindow>();
			win.Show();
		}

		private void OnEnable() {
			var shaderTiers = AssetDatabase.LoadAssetAtPath<ShaderTierConfig>(ASSET_PATH);
			if( !shaderTiers ) {
				shaderTiers = CreateInstance<ShaderTierConfig>();
				AssetDatabase.CreateAsset(shaderTiers, ASSET_PATH);
			}

			var slObj = new SerializedObject(shaderTiers);
			var property = slObj.FindProperty("Tiers");
			m_shaderList = new ReorderableList(slObj, property) {
				drawHeaderCallback = rect => {
					EditorGUI.LabelField(rect, "Shader Lod Tiers");
				},
				drawElementCallback = (rect, index, active, focused) => {
					var shaderTierProperty = property.GetArrayElementAtIndex(index);
					var shaderProp = shaderTierProperty.FindPropertyRelative("Shader");
					rect.height = EditorGUIUtility.singleLineHeight;
					EditorGUI.PropertyField(rect, shaderProp);
				},
				onSelectCallback = list => {
					
				}
			};
		}

		private void OnGUI() {
			EditorGUI.BeginChangeCheck();
			var drawRect = new Rect(0, 0, position.width, position.height);
			var shaderListRect = drawRect;
			shaderListRect.width *= 0.3f;
			m_shaderList.DoList(shaderListRect);

			if( EditorGUI.EndChangeCheck() ) {
				m_shaderList.serializedProperty.serializedObject.ApplyModifiedProperties();
			}

			var toolBar = drawRect;
			toolBar.xMin = shaderListRect.xMax + 5;
			toolBar.height = EditorGUIUtility.singleLineHeight;
			GUI.Toolbar(toolBar, 0, new[] { "Tier1", "Tier2", "Tier3" });
		}
	}
}