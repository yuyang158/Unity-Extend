using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ABSystem.Editor {
	public class StaticAssetBundleWindow : EditorWindow {
		[MenuItem( "MyMenu/Do Something2" )]
		private static void Init() {
			var window = (StaticAssetBundleWindow) GetWindow( typeof(StaticAssetBundleWindow) );
			window.Show();
		}

		private ReorderableList reList;
		private StaticABSettings settingRoot;
		private const string SETTING_FILE_PATH = "ABSystem/Editor/settings.asset";

		private void OnEnable() {
			if( settingRoot == null ) {
				settingRoot = AssetDatabase.LoadAssetAtPath<StaticABSettings>( SETTING_FILE_PATH );
				if( settingRoot == null ) {
					settingRoot = CreateInstance<StaticABSettings>();
					AssetDatabase.CreateAsset( settingRoot, SETTING_FILE_PATH );
				}
			}
			
			reList = new ReorderableList( settingRoot.Settings, typeof(StaticABSetting) );
			reList.drawHeaderCallback += rect => {
				EditorGUI.LabelField( rect, "AB特殊处理列表" );
			};
			
			reList.drawElementCallback += (rect, index, active, focused) => {
				rect.y += (rect.height - EditorGUIUtility.singleLineHeight) / 2;
				rect.height = EditorGUIUtility.singleLineHeight;
				var totalWidth = position.width;
				rect.width = totalWidth / 2;
				
				var setting = settingRoot.Settings[index];
				var asset = EditorGUI.ObjectField( rect, "路径", setting.FolderPath, typeof(DefaultAsset), false ) as DefaultAsset;
				if( asset != setting.FolderPath ) {
					var path = AssetDatabase.GetAssetPath( asset );
					if( System.IO.Directory.Exists( path ) ) {
						setting.FolderPath = asset;
					}
				}

				rect.x += rect.width + 5;
				rect.width = totalWidth - rect.x;
				setting.Op = (StaticABSetting.Operation)EditorGUI.EnumPopup( rect, "操作", setting.Op );
			};
		}

		private void OnGUI() {
			reList.DoLayoutList();

			var rect = EditorGUILayout.BeginHorizontal();
			rect.width = 100;
			rect.height = EditorGUIUtility.singleLineHeight;
			if( GUI.Button( rect, "Start Build" ) ) {
				BuildAssetRelation.BuildRelation();
			}
			
			if(GUI.Button( rect, "" ))
			EditorGUILayout.EndHorizontal();
		}
	}
}