using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Extend.Asset.Editor {
	public class StaticAssetBundleWindow : EditorWindow {
		[MenuItem("Window/AB Builder")]
		private static void Init() {
			var window = (StaticAssetBundleWindow)GetWindow(typeof(StaticAssetBundleWindow));
			window.Show();
		}

		private ReorderableList reList;
		private ReorderableList otherDependencyList;
		private static StaticABSettings settingRoot;
		private SerializedObject serializedObject;
		public const string SETTING_FILE_PATH = "Assets/Extend/AssetService/Editor/settings.asset";

		private void OnEnable() {
			InitializeSetting();

			serializedObject = new SerializedObject(settingRoot);
			var settingsProp = serializedObject.FindProperty("Settings");
			reList = new ReorderableList(serializedObject, settingsProp);
			reList.drawHeaderCallback += rect => { EditorGUI.LabelField(rect, "AB特殊处理列表"); };
			reList.drawElementCallback += (rect, index, active, focused) => {
				rect.height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
				var totalWidth = position.width;
				rect.width = totalWidth / 2;
				
				var settingProp = settingsProp.GetArrayElementAtIndex(index);
				var pathProp = settingProp.FindPropertyRelative("FolderPath");
				EditorGUI.PropertyField(rect, pathProp, new GUIContent("路径"));

				rect.x += rect.width + 5;
				rect.width = totalWidth - rect.x;
				var opProp = settingProp.FindPropertyRelative("Op");
				EditorGUI.PropertyField(rect, opProp);
				serializedObject.ApplyModifiedProperties();
			};
			
			var otherDepend = serializedObject.FindProperty("ExtraDependencyAssets");
			otherDependencyList = new ReorderableList(otherDepend.serializedObject, otherDepend);
			otherDependencyList.drawHeaderCallback += rect => { EditorGUI.LabelField(rect, "非Resources依赖"); };
			otherDependencyList.drawElementCallback += (rect, index, active, focused) => {
				rect.height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
				var dependProp = otherDependencyList.serializedProperty.GetArrayElementAtIndex(index);
				EditorGUI.BeginChangeCheck();
				EditorGUI.PropertyField(rect, dependProp);
				if( EditorGUI.EndChangeCheck() && dependProp.objectReferenceValue != null ) {
					var path = AssetDatabase.GetAssetPath(dependProp.objectReferenceValue);
					if( string.IsNullOrEmpty(path) || path.ToLower().StartsWith("assets/resources") ) {
						dependProp.objectReferenceValue = null;
					}
				}
				serializedObject.ApplyModifiedProperties();
			};
		}

		private static void InitializeSetting() {
			if( settingRoot == null ) {
				settingRoot = AssetDatabase.LoadAssetAtPath<StaticABSettings>(SETTING_FILE_PATH);
				if( settingRoot == null ) {
					settingRoot = CreateInstance<StaticABSettings>();
					AssetDatabase.CreateAsset(settingRoot, SETTING_FILE_PATH);
				}
			}
		}

		private const int BOTTOM_BUTTON_COUNT = 3;

		private void OnGUI() {
			var atlasProp = serializedObject.FindProperty("SpriteAtlasFolder");
			EditorGUILayout.PropertyField(atlasProp, new GUIContent("Atlas Root"));
			EditorGUILayout.Space();

			reList.DoLayoutList();
			EditorGUILayout.Space();
			
			otherDependencyList.DoLayoutList();
			EditorGUILayout.Space();

			var rect = EditorGUILayout.BeginHorizontal();
			var segmentWidth = position.width / BOTTOM_BUTTON_COUNT;
			rect.height = EditorGUIUtility.singleLineHeight;
			rect.x = 5;
			rect.width = segmentWidth - 10;
			if( GUI.Button(rect, "Rebuild All AB") ) {
				RebuildAllAssetBundles(EditorUserBuildSettings.activeBuildTarget);
			}

			rect.x += segmentWidth;
			if( GUI.Button(rect, "Build Update AB") ) {
				BuildUpdateAssetBundles(EditorUserBuildSettings.activeBuildTarget);
			}

			rect.x += segmentWidth;
			if( GUI.Button(rect, "Save Config") ) {
				AssetDatabase.SaveAssets();
			}
			EditorGUILayout.EndHorizontal();
			serializedObject.ApplyModifiedProperties();
		}

		private static string buildRoot;
		private static string outputPath;

		private static string MakeOutputDirectory(BuildTarget buildTarget) {
			buildRoot = $"{Application.streamingAssetsPath}/ABBuild";
			if( !Directory.Exists(buildRoot) ) {
				Directory.CreateDirectory(buildRoot);
			}

			outputPath = $"{buildRoot}/{buildTarget.ToString()}";
			if( !Directory.Exists(outputPath) ) {
				Directory.CreateDirectory(outputPath);
			}

			return outputPath;
		}

		private static void ExportResourcesPackageConf() {
			// generate resources file to ab map config file
			using( var writer = new StreamWriter($"{outputPath}/package.conf") ) {
				foreach( var resourcesNode in BuildAssetRelation.ResourcesNodes ) {
					var guid = AssetDatabase.AssetPathToGUID(resourcesNode.AssetPath);
					var assetPath = resourcesNode.AssetPath.ToLower();
					writer.WriteLine($"{assetPath}|{resourcesNode.AssetBundleName}|{guid}");
				}
			}
		}

		public static void BuildUpdateAssetBundles(BuildTarget target, string versionFilePath = null) {
			InitializeSetting();
			var outputDir = MakeOutputDirectory(target);
			if( string.IsNullOrEmpty(versionFilePath) ) {
				versionFilePath = EditorUtility.OpenFilePanel("Open version file", buildRoot, "bin");
				if( string.IsNullOrEmpty(versionFilePath) )
					return;
			}

			BuildAtlasAB();
			BuildAssetRelation.Clear();
			var allABs = BuildAssetRelation.BuildBaseVersionData(versionFilePath);
			BuildAssetRelation.BuildRelation(settingRoot.Settings, spritesInAtlas, () => {
				ExportResourcesPackageConf();
				var manifest = BuildPipeline.BuildAssetBundles(outputPath,
					BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.IgnoreTypeTreeChanges,
					EditorUserBuildSettings.activeBuildTarget);

				versionFilePath = $"{buildRoot}/content_version.bin";
				BuildVersionFile(versionFilePath, manifest, outputPath);

				var needUpdateAssetBundles = new List<string>();
				foreach( var abName in manifest.GetAllAssetBundles() ) {
					BuildPipeline.GetCRCForAssetBundle(Path.Combine(outputPath, abName), out var currentCrc32);
					if( allABs.TryGetValue(abName, out var crc3d) && crc3d == currentCrc32)
						continue;
					needUpdateAssetBundles.Add(abName);
				}

				var date = DateTime.Now;
				var localTime = date.ToLocalTime();
				var dateString = $"{localTime.Year}-{localTime.Month}-{localTime.Day}_{localTime.Hour}-{localTime.Minute}-{localTime.Second}";
				outputDir += $"/update_{dateString}.txt";
				using( var writer = new StreamWriter(outputDir) ) {
					foreach( var needUpdateABName in needUpdateAssetBundles ) {
						var fileInfo = new FileInfo(Path.Combine(outputPath, needUpdateABName));
						writer.WriteLine($"{needUpdateABName}:{fileInfo.Length}");
					}
				}
			});
		}

		private static readonly HashSet<string> spritesInAtlas = new HashSet<string>();

		private static void BuildAtlasAB() {
			spritesInAtlas.Clear();
			if(!settingRoot.SpriteAtlasFolder)
				return;
			var atlasDirectory = AssetDatabase.GetAssetPath(settingRoot.SpriteAtlasFolder);
			var atlases = Directory.GetFiles(atlasDirectory, "*.spriteatlas");
			foreach( var atlas in atlases ) {
				var directory = Path.GetDirectoryName(atlas) ?? "";
				var abName = Path.Combine(directory, Path.GetFileNameWithoutExtension(atlas));
				var dependencies = AssetDatabase.GetDependencies(atlas);
				foreach( var dependency in dependencies ) {
					var spriteImporter = AssetImporter.GetAtPath(dependency);
					if( spriteImporter is TextureImporter ) {
						spriteImporter.assetBundleName = abName;
						spritesInAtlas.Add(dependency.ToLower());
					}
				}
			}
		}

		public static void RebuildAllAssetBundles(BuildTarget target, Action finishCallback = null) {
			InitializeSetting();
			MakeOutputDirectory(target);
			BuildAssetRelation.Clear();
			BuildAtlasAB();
			
			BuildAssetRelation.BuildRelation(settingRoot.Settings, spritesInAtlas, () => {
				ExportResourcesPackageConf();

				var manifest = BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.DeterministicAssetBundle 
				                                                           | BuildAssetBundleOptions.ChunkBasedCompression, target);
				finishCallback?.Invoke();
				BuildVersionFile($"{buildRoot}/content_version.bin", manifest, outputPath);
			});
		}

		private static void BuildVersionFile(string versionFilePath, AssetBundleManifest manifest, string abOutputPath) {
			using( var fileStream = new FileStream(versionFilePath, FileMode.OpenOrCreate, FileAccess.Write) ) {
				using( var writer = new BinaryWriter(fileStream) ) {
					var assetBundles = manifest.GetAllAssetBundles();
					foreach( var abName in assetBundles ) {
						writer.Write(abName);
						var abPath = Path.Combine(abOutputPath, abName);
						BuildPipeline.GetCRCForAssetBundle(abPath, out var crc32);
						writer.Write(crc32);
					}
				}
			}
		}
	}
}