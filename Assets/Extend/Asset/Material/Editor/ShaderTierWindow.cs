using System;
using System.Collections.Generic;
using Extend.Common.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Extend.Asset.Material.Editor {
	public class ShaderTierWindow : EditorWindow {
		private ReorderableList m_shaderList;
		private const string ASSET_PATH = "Assets/Extend/Asset/Material/Editor/ShaderTiers.asset";
		private SerializedProperty m_selectedProperty;

		[MenuItem("Tools/Shader Lod Tier")]
		private static void ShowWindow() {
			var win = CreateWindow<ShaderTierWindow>();
			win.titleContent = new GUIContent("Shader Lod Tier Keyword");
			win.Show();
		}

		private Vector2 m_activeKeywordPosition;
		private Vector2 m_disabledKeywordPosition;

		private void OnEnable() {
			var shaders = AssetDatabase.LoadAssetAtPath<ShaderTierConfig>(ASSET_PATH);
			if( !shaders ) {
				shaders = CreateInstance<ShaderTierConfig>();
				AssetDatabase.CreateAsset(shaders, ASSET_PATH);
			}

			var slObj = new SerializedObject(shaders);
			var property = slObj.FindProperty("Shaders");
			m_shaderList = new ReorderableList(slObj, property) {
				drawHeaderCallback = rect => { EditorGUI.LabelField(rect, "Shaders"); },
				drawElementCallback = (rect, index, active, focused) => {
					var width = EditorGUIUtility.labelWidth;
					EditorGUIUtility.labelWidth = 55;
					var shaderTierProperty = property.GetArrayElementAtIndex(index);
					var shaderProp = shaderTierProperty.FindPropertyRelative("Shader");
					rect.height = EditorGUIUtility.singleLineHeight;
					EditorGUI.PropertyField(rect, shaderProp);
					EditorGUIUtility.labelWidth = width;
				},
				onSelectCallback = list => { m_selectedProperty = property.GetArrayElementAtIndex(list.index); }
			};
		}

		private int m_tierSelected;

		private void OnGUI() {
			EditorGUI.BeginChangeCheck();
			var drawRect = new Rect(0, 0, position.width, position.height);
			var shaderListRect = drawRect;
			shaderListRect.width *= 0.3f;
			m_shaderList.DoList(shaderListRect);

			if( EditorGUI.EndChangeCheck() ) {
				m_shaderList.serializedProperty.serializedObject.ApplyModifiedProperties();
			}

			if( m_selectedProperty == null ) {
				return;
			}

			var shaderProp = m_selectedProperty.FindPropertyRelative("Shader");
			var shader = shaderProp.objectReferenceValue as Shader;

			if( !shader ) {
				return;
			}

			EditorGUI.BeginChangeCheck();
			var toolBarRect = drawRect;
			toolBarRect.xMin = shaderListRect.xMax + 5;
			toolBarRect.height = EditorGUIUtility.singleLineHeight;
			m_tierSelected = GUI.Toolbar(toolBarRect, m_tierSelected, new[] { "Tier1", "Tier2", "Tier3" });

			List<string> keywords = new List<string>();
			keywords.AddRange(ShaderUtilExtend.GetGlobalShaderKeywords(shader));
			keywords.AddRange(ShaderUtilExtend.GetLocalShaderKeywords(shader));

			var tiersProperty = m_selectedProperty.FindPropertyRelative("Tiers");
			while( tiersProperty.arraySize < 3 ) {
				tiersProperty.InsertArrayElementAtIndex(tiersProperty.arraySize);
			}

			var tierProperty = tiersProperty.GetArrayElementAtIndex(m_tierSelected);
			var disabledKeywordsProperty = tierProperty.FindPropertyRelative("Keywords");
			for( int i = 0; i < disabledKeywordsProperty.arraySize; i++ ) {
				var keywordProperty = disabledKeywordsProperty.GetArrayElementAtIndex(i);
				var index = keywords.IndexOf(keywordProperty.stringValue);
				if( index == -1 ) {
					disabledKeywordsProperty.DeleteArrayElementAtIndex(i);
				}
				else {
					keywords[index] = "";
				}
			}

			var activeKeywordRect = drawRect;
			activeKeywordRect.xMin = shaderListRect.xMax + 5;
			activeKeywordRect.yMin = toolBarRect.yMax + 5;
			activeKeywordRect.width *= 0.5f;
			activeKeywordRect.width -= 2.5f;

			var activeKeywordViewRect = activeKeywordRect;
			activeKeywordViewRect.yMin += 5;
			activeKeywordViewRect.yMax -= 5;
			var keywordRect = activeKeywordViewRect;
			keywordRect.height = EditorGUIUtility.singleLineHeight;

			foreach( var keyword in keywords ) {
				if( string.IsNullOrEmpty(keyword) )
					continue;
				if( GUI.Button(keywordRect, keyword) ) {
					disabledKeywordsProperty.InsertArrayElementAtIndex(disabledKeywordsProperty.arraySize);
					var newAddProperty = disabledKeywordsProperty.GetArrayElementAtIndex(disabledKeywordsProperty.arraySize - 1);
					newAddProperty.stringValue = keyword;
				}

				keywordRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			}

			activeKeywordViewRect.x += activeKeywordViewRect.width + 5;
			keywordRect = activeKeywordViewRect;
			keywordRect.height = EditorGUIUtility.singleLineHeight;
			for( int i = 0; i < disabledKeywordsProperty.arraySize; i++ ) {
				var disableKeywordProperty = disabledKeywordsProperty.GetArrayElementAtIndex(i);
				if( GUI.Button(keywordRect, disableKeywordProperty.stringValue) ) {
					disabledKeywordsProperty.DeleteArrayElementAtIndex(i);
				}

				keywordRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			}

			if( EditorGUI.EndChangeCheck() ) {
				m_shaderList.serializedProperty.serializedObject.ApplyModifiedProperties();
			}
		}
	}
}