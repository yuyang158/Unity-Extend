using System;
using System.Collections;
using System.IO;
using System.Linq;
using CSObjectWrapEditor;
using Extend.Common.Editor;
using Unity.EditorCoroutines.Editor;
using Unity.SharpZipLib.Utils;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Extend.Editor {
	public static class ExtendEditorUtility {
		[MenuItem("Tools/常用工具/打开Log目录")]
		private static void OpenPersistencePath() {
			EditorUtility.RevealInFinder(Application.persistentDataPath);
		}
		
		[MenuItem("Tools/常用工具/Find Missing MonoBehaviour In Scene")]
		private static void FindMissingMonoBehaviourInScene() {
			for( int i = 0; i < SceneManager.sceneCount; i++ ) {
				var scene = SceneManager.GetSceneAt(i);
				var rootGameObjects = scene.GetRootGameObjects();
				foreach( var rootGameObject in rootGameObjects ) {
					FindMissingMonoBehaviourInTransform(rootGameObject.transform);
				}
			}
		}
		
		[MenuItem("Tools/常用工具/Find Missing MonoBehaviour In Prefab")]
		private static void FindMissingMonoBehaviourInPrefab() {
			var prefabs = Directory.GetFiles(Application.dataPath, "*.prefab", SearchOption.AllDirectories);
			for( int i = 0; i < prefabs.Length; i++ ) {
				var prefabPath = prefabs[i][(Application.dataPath.Length - 6)..];
				var go = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
				FindMissingMonoBehaviourInTransform(go.transform, true);
				EditorUtility.DisplayProgressBar("Missing Finding", $"Progress {i + 1} / {prefabs.Length}", (i + 1) / (float)prefabs.Length);
			}
			
			EditorUtility.ClearProgressBar();
		}

		private static void FindMissingMonoBehaviourInTransform(Transform t, bool selectPrefabRoot = false) {
			var components = t.GetComponents<Component>();
			if( components.Any(component => !component) ) {
				if( selectPrefabRoot ) {
					var root = PrefabUtility.GetNearestPrefabInstanceRoot(t.gameObject);
					Selection.activeObject = root;
					EditorGUIUtility.PingObject(root);
				}
				else {
					Selection.activeObject = t.gameObject;
					EditorGUIUtility.PingObject(Selection.activeObject);
				}
				return;
			}

			for( int i = 0; i < t.childCount; i++ ) {
				FindMissingMonoBehaviourInTransform(t.GetChild(i), selectPrefabRoot);
			}
		}

		private static void GenerateXLua() {
			Generator.ClearAll();
			Generator.GenAll();
		}

		private static void CheckEnvParamMatch(ref BuildOptions options, string argToMatch, BuildOptions enumValue) {
			if( Array.IndexOf(Environment.GetCommandLineArgs(), argToMatch) != -1 ) {
				options |= enumValue;
				Debug.Log($"Add Build Option {enumValue}");
			}
		}

		private static BuildOptions AnalyseCommandLineArgs() {
			var options = BuildOptions.None;
			CheckEnvParamMatch(ref options, "dev", BuildOptions.Development);
			CheckEnvParamMatch(ref options, "debug", BuildOptions.AllowDebugging);
			CheckEnvParamMatch(ref options, "scriptOnly", BuildOptions.BuildScriptsOnly);
			CheckEnvParamMatch(ref options, "compress", BuildOptions.CompressWithLz4);
			CheckEnvParamMatch(ref options, "deep", BuildOptions.EnableDeepProfilingSupport);
			CheckEnvParamMatch(ref options, "strict", BuildOptions.StrictMode);
			return options;
		}

		private static void ExportPlayerPrepare() {
			Debug.LogWarning($"Command Line : {Environment.CommandLine}");
			Debug.LogWarning($"Current Platform : {EditorUserBuildSettings.activeBuildTarget}");
		}
		
		private static string buildScript 
			= "Assets/AddressableAssetsData/DataBuilders/BuildScriptPackedMode.asset";
		private static string profileName = "DBLikeProfile";
		public static void ChangeSettings() {
			string defines = "";
			string[] args = Environment.GetCommandLineArgs();

			foreach (var arg in args)
				if (arg.StartsWith("-defines=", StringComparison.CurrentCulture))
					defines = arg.Substring(("-defines=".Length));

			var buildSettings = EditorUserBuildSettings.selectedBuildTargetGroup;
			PlayerSettings.SetScriptingDefineSymbolsForGroup(buildSettings, defines);
		}
		
		private static void BuildContentAndPlayer(string playerOutputPath) {
			var settings = AddressableAssetSettingsDefaultObject.Settings;
			settings.activeProfileId = settings.profileSettings.GetProfileId(profileName);

			var builder = AssetDatabase.LoadAssetAtPath<ScriptableObject>(buildScript) as IDataBuilder;
			settings.ActivePlayerDataBuilderIndex = settings.DataBuilders.IndexOf((ScriptableObject)builder);

			/*AddressableAssetSettings.BuildPlayerContent(out var result);

			if (!string.IsNullOrEmpty(result.Error))
				throw new Exception(result.Error);*/

			var buildReport = BuildPipeline.BuildPlayer(EditorBuildSettings.scenes,
					playerOutputPath, EditorUserBuildSettings.activeBuildTarget, BuildOptions.None);

			if (buildReport.summary.result != BuildResult.Succeeded)
				throw new Exception(buildReport.summary.ToString());
		}


		[MenuItem("Tools/CI/Build Android Player")]
		private static void ExportAndroid() {
			Debug.Log("Start Export Android Project");
			ExportPlayerPrepare();
			if( EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android ) {
				EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
			}
			EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
			EditorUserBuildSettings.exportAsGoogleAndroidProject = true;

			var buildPath = Application.dataPath + "/../AndroidPlayer";
			if( !Directory.Exists(buildPath) ) {
				Directory.CreateDirectory(buildPath);
			}

			Debug.Log("Gen XLUA Wrap");
			GenerateXLua();
			Debug.Log($"Start Build Player To : {buildPath}");
			BuildContentAndPlayer(buildPath);
			Debug.Log("Build Player Finished");

			if( Application.isBatchMode ) {
				EditorApplication.Exit(0);
			}
		}

		[MenuItem("Tools/CI/Build Win Player")]
		private static void ExportWin() {
			Debug.Log("Start Export Win Project");
			ExportPlayerPrepare();
			if( EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64 ) {
				EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
			}
			var buildPath = Application.dataPath + "/../WinPlayer";
			Debug.Log("Gen XLUA Wrap");
			GenerateXLua();
			Debug.Log($"Start Build Player To : {buildPath}");
			BuildContentAndPlayer(buildPath);
			Debug.Log("Build Player Finished");

			if( Application.isBatchMode ) {
				EditorCoroutineUtility.StartCoroutineOwnerless(DelayExit());
			}
		}


		[MenuItem("Tools/CI/Build iOS Player")]
		private static void ExportIOS() {
			Debug.Log("Start Export iOS Project");
			ExportPlayerPrepare();
			if( EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS ) {
				EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
			}
			EditorUserBuildSettings.iOSXcodeBuildConfig = XcodeBuildConfig.Release;
			var buildPath = Application.dataPath + "/../iOSPlayer";
			if( !Directory.Exists(buildPath) ) {
				Directory.CreateDirectory(buildPath);
			}

			var buildOptions = new BuildPlayerOptions {
				options = AnalyseCommandLineArgs(),
				target = BuildTarget.iOS,
				locationPathName = buildPath,
				// scenes = CollectScenesPath()
				scenes = Array.Empty<string>()
			};
			Debug.Log("Gen XLUA Wrap");
			GenerateXLua();
			Debug.Log($"Start Build Player To : {buildPath}");
			BuildPipeline.BuildPlayer(buildOptions);
			Debug.Log("Build Player Finished");

			if( Application.isBatchMode ) {
				EditorCoroutineUtility.StartCoroutineOwnerless(DelayExit());
			}
		}

		[PostProcessBuild(2)]
		private static void IOSPostBuild(BuildTarget buildTarget, string path) {
			if( buildTarget != BuildTarget.iOS )
				return;
#if UNITY_IOS
			
#endif
		}

		private static IEnumerator DelayExit() {
			yield return new WaitForSeconds(1);
			EditorApplication.Exit(0);
		}
		
		[MenuItem("Tools/Asset/GUID Convert")]
		public static void GUIDConvert() {
			var input = InputWindow.CreateWindow("Input GUID");
			input.Callback += s => {
				var path = AssetDatabase.GUIDToAssetPath(s);
				Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
			};
			input.ShowModal();
		}

		[MenuItem("Tools/Asset/Show Material Keywords")]
		private static void ShowWindow() {
			var mat = Selection.activeObject as Material;
			if( !mat )
				return;

			Debug.Log(string.Join(";", mat.shaderKeywords));
		}

		[MenuItem("Tools/Asset/Lua Pack Zip")]
		private static void PackLuaZip() {
			ZipUtility.CompressFolderToZip("./Lua.zip", Application.productName, $"{Application.dataPath}/../Lua");
			File.Copy("./Lua.zip", Application.streamingAssetsPath + "/Lua.zip", true);
		}

		[MenuItem("Tools/Addressable/Clear Output Directory")]
		private static void ClearOutputDirectory() {
			
		}
	}
}