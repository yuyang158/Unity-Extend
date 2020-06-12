using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Extend.Common;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Assertions;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace Extend.Asset.Editor {
	public class StaticAssetBundleWindow : EditorWindow {
		[MenuItem("Window/AB Builder")]
		private static void Init() {
			var window = (StaticAssetBundleWindow)GetWindow(typeof(StaticAssetBundleWindow));
			window.titleContent = new GUIContent("Asset Bundle Build");
			window.Show();
		}

		private ReorderableList reList;
		private ReorderableList otherDependencyList;
		private ReorderableList reSpecialUnloadStrategyList;
		private static StaticABSettings settingRoot;
		private SerializedObject serializedObject;
		private SerializedProperty selectedSetting;
		private readonly List<string> selectSettingABPaths = new List<string>();
		public const string SETTING_FILE_PATH = "Assets/Extend/Asset/Editor/settings.asset";

		private static readonly GUIContent SPECIAL_LIST_HEADER = new GUIContent("Special Folder List");
		private static readonly GUIContent PATH_CONTENT = new GUIContent("Path");
		private static readonly GUIContent ASSET_BUNDLE_CONTENT = new GUIContent("AB Name");
		private static readonly GUIContent UNLOAD_STRATEGY_CONTENT = new GUIContent("Strategy");
		private static readonly GUIContent OPERATION_CONTENT = new GUIContent("Operation");

		private void FormatAndAddSpecialSettingPath(string path) {
			path = path.ToLower().Replace('\\', '/');
			Assert.IsTrue(path.StartsWith("assets"));
			selectSettingABPaths.Add(path);
		}

		private void OnEnable() {
			InitializeSetting();

			serializedObject = new SerializedObject(settingRoot);
			var settingsProp = serializedObject.FindProperty("Settings");
			reList = new ReorderableList(serializedObject, settingsProp);
			reList.drawHeaderCallback += rect => { EditorGUI.LabelField(rect, SPECIAL_LIST_HEADER); };
			reList.drawElementCallback += (rect, index, active, focused) => {
				rect.height = EditorGUIUtility.singleLineHeight;
				var totalWidth = position.width;
				rect.width = totalWidth / 2;

				var labelWidth = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 70;
				var settingProp = settingsProp.GetArrayElementAtIndex(index);
				var pathProp = settingProp.FindPropertyRelative("FolderPath");
				EditorGUI.PropertyField(rect, pathProp, PATH_CONTENT);

				rect.x += rect.width + 5;
				rect.width = totalWidth - rect.x;
				var opProp = settingProp.FindPropertyRelative("Op");
				EditorGUI.PropertyField(rect, opProp, OPERATION_CONTENT);
				serializedObject.ApplyModifiedProperties();
				EditorGUIUtility.labelWidth = labelWidth;
			};

			reList.onSelectCallback += list => {
				if( list.index >= 0 && list.index < list.count ) {
					selectedSetting = list.serializedProperty.GetArrayElementAtIndex(list.index);
					var folderProp = selectedSetting.FindPropertyRelative("FolderPath");
					var folderPath = folderProp.objectReferenceValue ? AssetDatabase.GetAssetPath(folderProp.objectReferenceValue) : string.Empty;
					selectSettingABPaths.Clear();
					if( string.IsNullOrEmpty(folderPath) ) {
						reSpecialUnloadStrategyList = null;
						return;
					}

					var opEnumProp = selectedSetting.FindPropertyRelative("Op");
					switch( (StaticABSetting.Operation)opEnumProp.intValue ) {
						case StaticABSetting.Operation.ALL_IN_ONE:
							FormatAndAddSpecialSettingPath(folderPath.ToLower());
							break;
						case StaticABSetting.Operation.STAY_RESOURCES:
							break;
						case StaticABSetting.Operation.EACH_FOLDER_ONE:
							var subDirectories = Directory.GetDirectories(folderPath);
							foreach( var subDirectory in subDirectories ) {
								FormatAndAddSpecialSettingPath($"{folderPath}/{Path.GetDirectoryName(subDirectory)}");
							}

							break;
						case StaticABSetting.Operation.EACH_A_AB:
							var files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
							foreach( var file in files ) {
								if( file.StartsWith(".") || Path.GetExtension(file) == ".meta" ) {
									continue;
								}

								FormatAndAddSpecialSettingPath($"{folderPath}/{Path.GetFileNameWithoutExtension(file)}");
							}

							break;
						default:
							throw new ArgumentOutOfRangeException();
					}

					var unloadStrategyProp = selectedSetting.FindPropertyRelative("UnloadStrategies");
					for( var i = 0; i < unloadStrategyProp.arraySize; ) {
						var elementProp = unloadStrategyProp.GetArrayElementAtIndex(i);
						var bundleNameProp = elementProp.FindPropertyRelative("BundleName");
						var index = selectSettingABPaths.IndexOf(bundleNameProp.stringValue);
						if( index < 0 ) {
							unloadStrategyProp.DeleteArrayElementAtIndex(i);
						}
						else {
							selectSettingABPaths.RemoveSwapAt(index);
							i++;
						}
					}

					foreach( var path in selectSettingABPaths ) {
						unloadStrategyProp.InsertArrayElementAtIndex(unloadStrategyProp.arraySize);
						var loadProp = unloadStrategyProp.GetArrayElementAtIndex(unloadStrategyProp.arraySize - 1);
						var bundleNameProp = loadProp.FindPropertyRelative("BundleName");
						bundleNameProp.stringValue = path;
					}

					reSpecialUnloadStrategyList = new ReorderableList(serializedObject, unloadStrategyProp);
					reSpecialUnloadStrategyList.drawHeaderCallback += rect => { EditorGUI.LabelField(rect, "Unload Strategy"); };
					reSpecialUnloadStrategyList.drawElementCallback += (rect, index, active, focused) => {
						var element = unloadStrategyProp.GetArrayElementAtIndex(index);
						rect.height = EditorGUIUtility.singleLineHeight;
						var totalWidth = position.width;
						rect.width = totalWidth / 2;

						GUI.enabled = false;
						var labelWidth = EditorGUIUtility.labelWidth;
						EditorGUIUtility.labelWidth = 70;
						var pathProp = element.FindPropertyRelative("BundleName");
						EditorGUI.PropertyField(rect, pathProp, ASSET_BUNDLE_CONTENT);
						GUI.enabled = true;

						rect.x += rect.width + 5;
						rect.width = totalWidth - rect.x;
						var strategyProp = element.FindPropertyRelative("UnloadStrategy");
						EditorGUI.PropertyField(rect, strategyProp, UNLOAD_STRATEGY_CONTENT);
						serializedObject.ApplyModifiedProperties();
						EditorGUIUtility.labelWidth = labelWidth;
					};
				}
				else {
					selectedSetting = null;
				}
			};

			var otherDepend = serializedObject.FindProperty("ExtraDependencyAssets");
			otherDependencyList = new ReorderableList(otherDepend.serializedObject, otherDepend);
			otherDependencyList.drawHeaderCallback += rect => { EditorGUI.LabelField(rect, "非Resources依赖"); };
			otherDependencyList.drawElementCallback += (rect, index, active, focused) => {
				rect.height = EditorGUIUtility.singleLineHeight;
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
			reList.DoLayoutList();
			EditorGUILayout.Space();

			if( selectedSetting != null && reSpecialUnloadStrategyList != null ) {
				EditorGUILayout.Space();
				reSpecialUnloadStrategyList.DoLayoutList();
			}

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
		public static string OutputPath => outputPath;

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

		public static void BuildUpdateAssetBundles(BuildTarget target, string versionFilePath = null) {
			InitializeSetting();
			var outputDir = MakeOutputDirectory(target);
			if( string.IsNullOrEmpty(versionFilePath) ) {
				versionFilePath = EditorUtility.OpenFilePanel("Open version file", buildRoot, "bin");
				if( string.IsNullOrEmpty(versionFilePath) )
					return;
			}

			BuildAssetRelation.Clear();
			var allABs = BuildAssetRelation.BuildBaseVersionData(versionFilePath);
			BuildAssetRelation.BuildRelation(settingRoot, () => {
				var manifest = BuildPipeline.BuildAssetBundles(outputPath,
					BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.ChunkBasedCompression |
					BuildAssetBundleOptions.IgnoreTypeTreeChanges,
					EditorUserBuildSettings.activeBuildTarget);

				versionFilePath = $"{buildRoot}/content_version.bin";
				BuildVersionFile(versionFilePath, manifest, outputPath);

				var needUpdateAssetBundles = new List<string>();
				foreach( var abName in manifest.GetAllAssetBundles() ) {
					BuildPipeline.GetCRCForAssetBundle(Path.Combine(outputPath, abName), out var currentCrc32);
					if( allABs.TryGetValue(abName, out var crc3d) && crc3d == currentCrc32 )
						continue;
					needUpdateAssetBundles.Add(abName);
				}

				if(needUpdateAssetBundles.Count == 0)
					return;

				var updateFolderPath = $"{Application.dataPath}/../UpdateAssets";
				if( Directory.Exists(updateFolderPath) ) {
					Directory.Delete(updateFolderPath, true);
				}
				Directory.CreateDirectory(updateFolderPath);
				foreach( var needUpdateABName in needUpdateAssetBundles ) {
					var fileInfo = new FileInfo(Path.Combine(outputPath, needUpdateABName));
					var destInfo = new FileInfo(Path.Combine(updateFolderPath, needUpdateABName));
					destInfo.Directory.Create();
					fileInfo.CopyTo(destInfo.FullName, true);
				}

				ZipFile.CreateFromDirectory(updateFolderPath, $"{updateFolderPath}/../{DateTime.Now.ToLongDateString()}.zip", 
					CompressionLevel.NoCompression, false);
			});
		}

		public static void RebuildAllAssetBundles(BuildTarget target, Action finishCallback = null) {
			InitializeSetting();
			MakeOutputDirectory(target);
			BuildAssetRelation.Clear();
			BuildAssetRelation.BuildRelation(settingRoot, () => {
				var manifest = BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.DeterministicAssetBundle
				                                                           | BuildAssetBundleOptions.ChunkBasedCompression, target);
				finishCallback?.Invoke();
				BuildVersionFile($"{buildRoot}/content_version.bin", manifest, outputPath);
			});
		}

		private static void BuildVersionFile(string versionFilePath, AssetBundleManifest manifest, string abOutputPath) {
			using( var fileStream = new FileStream(versionFilePath, FileMode.OpenOrCreate, FileAccess.Write) ) {
				using( var writer = new StreamWriter(fileStream) ) {
					var assetBundles = manifest.GetAllAssetBundles();
					foreach( var abName in assetBundles ) {
						var abPath = Path.Combine(abOutputPath, abName);
						BuildPipeline.GetCRCForAssetBundle(abPath, out var crc32);
						writer.WriteLine($"{abName}|{crc32}");
					}
				}
			}
		}
	}
}