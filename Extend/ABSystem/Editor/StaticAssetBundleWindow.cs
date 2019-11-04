using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Directory = UnityEngine.Windows.Directory;

namespace ABSystem.Editor {
	public class StaticAssetBundleWindow : EditorWindow {
		[MenuItem( "Window/AB Builder" )]
		private static void Init() {
			var window = (StaticAssetBundleWindow) GetWindow( typeof(StaticAssetBundleWindow) );
			window.Show();
		}

		private ReorderableList reList;
		private StaticABSettings settingRoot;
		private const string SETTING_FILE_PATH = "Assets/Extend/ABSystem/Editor/settings.asset";

		private void OnEnable() {
			if( settingRoot == null ) {
				settingRoot = AssetDatabase.LoadAssetAtPath<StaticABSettings>( SETTING_FILE_PATH );
				if( settingRoot == null ) {
					settingRoot = CreateInstance<StaticABSettings>();
					AssetDatabase.CreateAsset( settingRoot, SETTING_FILE_PATH );
				}

				if( settingRoot.Settings == null ) {
					settingRoot.Settings = new List<StaticABSetting>();
				}
			}

			reList = new ReorderableList( settingRoot.Settings, typeof(StaticABSetting) );
			reList.drawHeaderCallback += rect => { EditorGUI.LabelField( rect, "AB特殊处理列表" ); };

			reList.drawElementCallback += (rect, index, active, focused) => {
				rect.y += ( rect.height - EditorGUIUtility.singleLineHeight ) / 2;
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
				setting.Op = (StaticABSetting.Operation) EditorGUI.EnumPopup( rect, "操作", setting.Op );
			};
		}

		private const int BOTTOM_BUTTON_COUNT = 3;

		private void OnGUI() {
			reList.DoLayoutList();

			var rect = EditorGUILayout.BeginHorizontal();
			var width = position.width / BOTTOM_BUTTON_COUNT;
			rect.height = EditorGUIUtility.singleLineHeight;
			rect.x = 5;
			rect.width = width - 5;
			if( GUI.Button( rect, "Build Whole New AB" ) ) {
				BuildWholeAssetBundles();
			}

			rect.x += width;
			if( GUI.Button( rect, "Build Update AB" ) ) {
				BuildUpdateAssetBundles();
			}

			rect.x += width;
			if( GUI.Button( rect, "Save Config" ) ) {
			}

			EditorGUILayout.EndHorizontal();
		}

		private static string buildRoot;
		private static string outputPath;

		private static string BuildOutputDirectory() {
			var buildTarget = EditorUserBuildSettings.activeBuildTarget;
			buildRoot = $"{Application.dataPath}/ABBuild";
			if( !Directory.Exists( buildRoot ) ) {
				Directory.CreateDirectory( buildRoot );
			}

			outputPath = $"{buildRoot}/{buildTarget.ToString()}";
			if( !Directory.Exists( outputPath ) ) {
				Directory.CreateDirectory( outputPath );
			}

			return outputPath;
		}

		private static void ExportResourcesPackageConf() {
			// generate resources file to ab map config file
			using( var writer = new StreamWriter( $"{outputPath}/package.conf" ) ) {
				foreach( var resourcesNode in BuildAssetRelation.ResourcesNodes ) {
					var assetPath = resourcesNode.AssetPath.ToLower();
					writer.WriteLine( $"{assetPath}|{resourcesNode.AssetBundleName}" );
				}
			}
		}

		public void BuildUpdateAssetBundles(string versionFilePath = null) {
			var outputDir = BuildOutputDirectory();
			if( string.IsNullOrEmpty( versionFilePath ) ) {
				versionFilePath = EditorUtility.OpenFilePanel( "Open version file", buildRoot, "bin" );
				if( string.IsNullOrEmpty( versionFilePath ) )
					return;
			}

			BuildAssetRelation.Clear();
			var allABs = BuildAssetRelation.BuildBaseVersionData( versionFilePath );
			BuildAssetRelation.BuildRelation( settingRoot.Settings, () => {
				ExportResourcesPackageConf();
				var manifest = BuildPipeline.BuildAssetBundles( outputPath, BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.ChunkBasedCompression,
					EditorUserBuildSettings.activeBuildTarget );

				foreach( var abName in manifest.GetAllAssetBundles() ) {
					if( allABs.Contains( abName ) ) continue;
					BuildAssetRelation.NeedUpdateBundles.Add( abName );
					allABs.Add( abName );
				}

				var date = DateTime.Now;
				var localTime = date.ToLocalTime();
				var dateString = $"{localTime.Year}-{localTime.Month}-{localTime.Day}_{localTime.Hour}-{localTime.Minute}-{localTime.Second}";
				outputDir += $"update_{dateString}.txt";
				using( var writer = new StreamWriter( outputDir ) ) {
					foreach( var needUpdateABName in BuildAssetRelation.NeedUpdateBundles ) {
						var fileInfo = new FileInfo( needUpdateABName );
						writer.WriteLine( $"{needUpdateABName}:{fileInfo.Length}" );
					}
				}
			} );
		}

		public void BuildWholeAssetBundles() {
			BuildOutputDirectory();
			BuildAssetRelation.Clear();
			BuildAssetRelation.BuildRelation( settingRoot.Settings, () => {
				ExportResourcesPackageConf();
				using( var fileStream = new FileStream( $"{buildRoot}/contentversion.bin", FileMode.OpenOrCreate, FileAccess.Write ) ) {
					using( var writer = new BinaryWriter( fileStream ) ) {
						foreach( var node in BuildAssetRelation.AllNodes ) {
							writer.Write( node.GUID );
							writer.Write( node.AssetBundleName );
							writer.Write( node.AssetTimeStamp );
						}
					}
				}

				BuildPipeline.BuildAssetBundles( outputPath, BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.ChunkBasedCompression,
					EditorUserBuildSettings.activeBuildTarget );
			} );
		}
	}
}