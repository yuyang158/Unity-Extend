using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Extend.Asset.Editor.Process;
using Extend.Common;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

namespace Extend.Asset.Editor {
	public class StaticAssetBundleWindow : EditorWindow {
		[MenuItem("Window/AB Builder")]
		private static void Init() {
			var window = (StaticAssetBundleWindow)GetWindow(typeof(StaticAssetBundleWindow));
			window.titleContent = new GUIContent("Asset Bundle Build");
			window.Show();
		}

		private ReorderableList m_reList;
		private ReorderableList m_otherDependencyList;
		private ReorderableList m_reSpecialUnloadStrategyList;
		private ReorderableList m_sceneList;

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

			#region ABSetting

			m_reList = new ReorderableList(serializedObject, settingsProp);
			m_reList.drawHeaderCallback += rect => { EditorGUI.LabelField(rect, SPECIAL_LIST_HEADER, EditorStyles.boldLabel); };
			m_reList.drawElementCallback += (rect, index, active, focused) => {
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

			m_reList.onSelectCallback += list => {
				if( list.index >= 0 && list.index < list.count ) {
					selectedSetting = list.serializedProperty.GetArrayElementAtIndex(list.index);
					var folderProp = selectedSetting.FindPropertyRelative("FolderPath");
					var folderPath = folderProp.objectReferenceValue ? AssetDatabase.GetAssetPath(folderProp.objectReferenceValue) : string.Empty;
					selectSettingABPaths.Clear();
					if( string.IsNullOrEmpty(folderPath) ) {
						m_reSpecialUnloadStrategyList = null;
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

					m_reSpecialUnloadStrategyList = new ReorderableList(serializedObject, unloadStrategyProp);
					m_reSpecialUnloadStrategyList.drawHeaderCallback += rect => { EditorGUI.LabelField(rect, "Unload Strategy"); };
					m_reSpecialUnloadStrategyList.drawElementCallback += (rect, index, active, focused) => {
						var element = unloadStrategyProp.GetArrayElementAtIndex(index);
						rect.height = EditorGUIUtility.singleLineHeight;
						var totalWidth = position.width;
						rect.width = totalWidth / 2;

						GUI.enabled = false;
						var labelWidth = EditorGUIUtility.labelWidth;
						EditorGUIUtility.labelWidth = 70;
						var pathProp = element.FindPropertyRelative("BundleName");
						if( pathProp == null )
							return;
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

			#endregion

			#region OtherSetting

			var otherDepend = serializedObject.FindProperty("ExtraDependencyAssets");
			m_otherDependencyList = new ReorderableList(otherDepend.serializedObject, otherDepend);
			m_otherDependencyList.drawHeaderCallback += rect => { EditorGUI.LabelField(rect, "非Resources依赖", EditorStyles.boldLabel); };
			m_otherDependencyList.drawElementCallback += (rect, index, active, focused) => {
				rect.height = EditorGUIUtility.singleLineHeight;
				var dependProp = m_otherDependencyList.serializedProperty.GetArrayElementAtIndex(index);
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

			#endregion

			var scenesProp = serializedObject.FindProperty("Scenes");
			m_sceneList = new ReorderableList(serializedObject, scenesProp) {
				drawHeaderCallback = rect => { GUI.Label(rect, "Scenes", EditorStyles.boldLabel); },
				drawElementCallback = (rect, index, active, focused) => {
					var sceneProp = scenesProp.GetArrayElementAtIndex(index);
					EditorGUI.ObjectField(rect, sceneProp, GUIContent.none);
				} 
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
			m_reList.DoLayoutList();
			EditorGUILayout.Space();

			if( selectedSetting != null && m_reSpecialUnloadStrategyList != null ) {
				EditorGUILayout.Space();
				m_reSpecialUnloadStrategyList.DoLayoutList();
			}

			m_otherDependencyList.DoLayoutList();
			EditorGUILayout.Space();

			m_sceneList.DoLayoutList();
			EditorGUILayout.Space();

			var rect = EditorGUILayout.BeginHorizontal();
			var segmentWidth = position.width / BOTTOM_BUTTON_COUNT;
			rect.height = EditorGUIUtility.singleLineHeight;
			rect.x = 5;
			rect.width = segmentWidth - 10;
			if( GUI.Button(rect, "Rebuild All AB") ) {
				RebuildAllAssetBundles(EditorUserBuildSettings.activeBuildTarget, true);
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

		public static string OutputPath {
			get {
				if( string.IsNullOrEmpty(outputPath) ) {
					MakeOutputDirectory(m_currentBuildTarget);
				}

				return outputPath;
			}
		}

		private static string copyPath;

		public static string CopyPath
		{
			get
			{
				if (string.IsNullOrEmpty(copyPath))
				{
					MakeCopyDirectory(m_currentBuildTarget);
				}

				return copyPath;
			}
		}

		private static void MakeOutputDirectory(BuildTarget buildTarget) {
			buildRoot = $"{Application.dataPath}/../ABBuild";
			if( !Directory.Exists(buildRoot) ) {
				Directory.CreateDirectory(buildRoot);
			}

			outputPath = $"{buildRoot}/{buildTarget.ToString()}";
			if( !Directory.Exists(outputPath) ) {
				Directory.CreateDirectory(outputPath);
			}
		}
		private static void MakeCopyDirectory(BuildTarget buildTarget)
		{
			buildRoot = $"{Application.streamingAssetsPath}/ABBuild";
			if (!Directory.Exists(buildRoot))
			{
				Directory.CreateDirectory(buildRoot);
			}

			copyPath = $"{buildRoot}/{buildTarget.ToString()}";
			if (!Directory.Exists(copyPath))
			{
				Directory.CreateDirectory(copyPath);
			}
		}

		private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs) {
			// Get the subdirectories for the specified directory.
			var dir = new DirectoryInfo(sourceDirName);
			DirectoryInfo[] dirs = dir.GetDirectories();

			if( !dir.Exists ) {
				throw new DirectoryNotFoundException(
					"Source directory does not exist or could not be found: "
					+ sourceDirName);
			}

			if( dir.Name.StartsWith(".") ) {
				return;
			}

			// If the destination directory doesn't exist, create it.
			if( !Directory.Exists(destDirName) ) {
				Directory.CreateDirectory(destDirName);
			}

			// Get the files in the directory and copy them to the new location.
			FileInfo[] files = dir.GetFiles();
			foreach( var file in files ) {
				string tempPath = Path.Combine(destDirName, file.Name);
				file.CopyTo(tempPath, true);
			}

			// If copying subdirectories, copy them and their contents to new location.
			if( copySubDirs ) {
				foreach( var subdir in dirs ) {
					string tempPath = Path.Combine(destDirName, subdir.Name);
					DirectoryCopy(subdir.FullName, tempPath, true);
				}
			}
		}

		private static BuildTarget m_currentBuildTarget;

		/// <summary>
		/// build lua-like files(include '.lua' & '.proto')
		/// </summary>
		/// <param name="luaRootPath">absolute path</param>
		private static void BuildLuaFile(string luaRootPath)
		{
			System.Diagnostics.Process process = new System.Diagnostics.Process();
#if UNITY_EDITOR_WIN
			process.StartInfo.WorkingDirectory = Path.Combine(Application.dataPath.Replace("Assets", ""), "xlua_build", "luac", "build64", "Release").Replace("/", @"\");
			process.StartInfo.FileName = Path.Combine(process.StartInfo.WorkingDirectory, "build_lua.bat");
			process.StartInfo.Arguments = luaRootPath.Replace("/", @"\");
#else			
			process.StartInfo.WorkingDirectory = Path.Combine(Application.dataPath.Replace("Assets", ""), "xlua_build", "luac", "build_unix");
			process.StartInfo.FileName = "/bin/sh";
			process.StartInfo.Arguments = Path.Combine(process.StartInfo.WorkingDirectory, "build_lua.sh") + " " + luaRootPath;
#endif			
			process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.StandardOutputEncoding = UTF8Encoding.UTF8;
			process.StartInfo.StandardErrorEncoding = UTF8Encoding.UTF8;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.CreateNoWindow = true;
			process.Start();
			process.WaitForExit();
			var output = process.StandardOutput.ReadToEnd();
			var outputError = process.StandardError.ReadToEnd();
			if (!string.IsNullOrEmpty(output))
			{
				Debug.Log(output);
			}

			if (!string.IsNullOrEmpty(outputError))
			{
				Debug.LogError(outputError);
			}
			process.Close();
#if UNITY_EDITOR_WIN
			DirectoryCopy($"{Application.dataPath}/../xlua_build/luac/build64/Release/Lua", $"{Application.dataPath}/Resources/Lua", true);
#else
			DirectoryCopy($"{Application.dataPath}/../xlua_build/luac/build_unix/Lua", $"{Application.dataPath}/Resources/Lua", true);
#endif
		}
		
		public static bool RebuildSelectedAssetBundles(BuildTarget target, bool appendLuaDir, int[] selectedIds) {
			if( appendLuaDir ) {
				BuildLuaFile(Path.Combine(Application.dataPath.Replace("Assets", ""), "Lua"));
				AssetDatabase.Refresh();
			}

			var builds = new List<AssetBundleBuild>();
			using( var reader = new StreamReader("./build-relations.tsv") ) {
				while( !reader.EndOfStream ) {
					var line = reader.ReadLine();
					var parts = line.Split('\t');
					var index = int.Parse(parts[0]);
					if( !selectedIds.Contains(index) ) {
						continue;
					}
					
					var assetBundleName = parts[1];
					var build = new AssetBundleBuild {
						assetBundleName = assetBundleName
					};

					var assetCount = parts.Length - 2;
					build.assetNames = new string[assetCount];
					for( int i = 2; i < parts.Length; i++ ) {
						build.assetNames[i - 2] = parts[i];
					}
					
					builds.Add(build);
				}
			}
			BuildPipeline.BuildAssetBundles(OutputPath, builds.ToArray(),
				BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.ChunkBasedCompression, target);
			Directory.Delete($"{Application.dataPath}/Resources/Lua", true);
			FinalClear();
			return true;
		}

		public static bool RebuildAllAssetBundles(BuildTarget target, bool appendLuaDir) {
			outputPath = null;
			m_currentBuildTarget = target;
			InitializeSetting();
			BuildAssetRelation.Clear();
			var settings = settingRoot;
			if( appendLuaDir ) {
				BuildLuaFile(Path.Combine(Application.dataPath.Replace("Assets", ""), "Lua"));
				AssetDatabase.Refresh();

				settings = CreateInstance<StaticABSettings>();
				settings.ExtraDependencyAssets = settingRoot.ExtraDependencyAssets;
				settings.Settings = new StaticABSetting[settingRoot.Settings.Length + 1];
				Array.Copy(settingRoot.Settings, settings.Settings, settingRoot.Settings.Length);
				settings.Settings[settingRoot.Settings.Length] = new StaticABSetting() {
					FolderPath = AssetDatabase.LoadAssetAtPath<DefaultAsset>("Assets/Resources/Lua"),
					Op = StaticABSetting.Operation.ALL_IN_ONE,
					UnloadStrategies = new SpecialBundleLoadLogic[0]
				};
				settings.Scenes = settingRoot.Scenes;
				Debug.Log("Finish copy lua directory");
			}

			try {
				BuildAssetRelation.BuildRelation(settings);
				Debug.Log($"Start AB Build Output : {OutputPath}");

				var buildContext = AssetNodeCollector.Output();
				using( var stream = new StreamWriter("./build-relations.tsv") ) {
					for( int i = 0; i < buildContext.Length; i++ ) {
						var ctx = buildContext[i];
						stream.WriteLine($"{i}\t{ctx.assetBundleName}\t{string.Join("\t", ctx.assetNames)}");
					}
				}
				BuildPipeline.BuildAssetBundles(OutputPath, buildContext,
					BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.ChunkBasedCompression, target);
				Directory.Delete($"{Application.dataPath}/Resources/Lua", true);
				FinalClear();
				//处理之前先删除streamingAssetsPath内的内容，以免有冗余的旧ab包打进安装包
				if (Directory.Exists(CopyPath))
				{
					Directory.Delete(CopyPath, true);
					Directory.CreateDirectory(CopyPath);
				}
				foreach (var item in buildContext)
				{
					string abBuildOutPath = Path.Combine(OutputPath, item.assetBundleName);
					string nameStrHash = item.assetBundleName.GetHashCode().ToString();
					byte[] filedata = File.ReadAllBytes(abBuildOutPath);
					byte[] offsetData = Encoding.UTF8.GetBytes(nameStrHash);
					int offset = offsetData.Length;
					int filelen = offset + filedata.Length;
					byte[] buffer = new byte[filelen];
					CopyData(buffer, offsetData, 0);
					CopyData(buffer, filedata, offset);
					//Debug.Log($"{CopyPath}    {item.assetBundleName}     {Path.Combine(CopyPath, item.assetBundleName)}");
					string targetPath = Path.Combine(CopyPath, item.assetBundleName);
					string directoryPath = Path.GetDirectoryName(targetPath);
					if(!Directory.Exists(directoryPath))
					{
						Directory.CreateDirectory(directoryPath);
					}
					var fs = File.Open(targetPath, FileMode.OpenOrCreate, FileAccess.Write);
					fs.Write(buffer, 0, filelen);
					fs.Close();
				}
				File.Copy($"{OutputPath}/package.conf", $"{CopyPath}/package.conf", true);
				File.Copy($"{OutputPath}/{m_currentBuildTarget.ToString()}",
					$"{CopyPath}/{m_currentBuildTarget.ToString()}", true);
				return true;
			}
			catch( Exception e ) {
				EditorUtility.ClearProgressBar();
				Debug.LogException(e);
				return false;
			}
			finally {
				AssetCustomProcesses.Shutdown();
			}
			return true;
		}
		static void CopyData(byte[] buffer, byte[] data, int offset)
		{
			Array.Copy(data, 0, buffer, offset, data.Length);
		}

		private static void FinalClear() {
			AssetCustomProcesses.Clear();
			var manifestFiles = Directory.GetFiles(OutputPath, "*.manifest", SearchOption.AllDirectories);
			foreach( var manifestFile in manifestFiles ) {
				File.Delete(manifestFile);
			}
			
			var manifestMetaFiles = Directory.GetFiles(OutputPath, "*.manifest.meta", SearchOption.AllDirectories);
			foreach( var metaFile in manifestMetaFiles ) {
				File.Delete(metaFile);
			}

			AssetDatabase.Refresh();
		}
	}
}