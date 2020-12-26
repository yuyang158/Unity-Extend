using System;
using System.Collections;
using System.IO;
using CSObjectWrapEditor;
using Extend.Asset.Editor;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Extend.Editor {
	public static class ExtendEditorUtility {
		[MenuItem("Tools/常用工具/打开Log目录")]
		private static void OpenPersistencePath() {
			EditorUtility.RevealInFinder(Application.persistentDataPath);
		}

		private static void RebuildAllABForPlatform(BuildTarget target) {
			Debug.LogWarning($"Rebuild all ab for platform {target}");
			StaticAssetBundleWindow.RebuildAllAssetBundles(target, true, () => {
				Debug.Log($"Rebuild all ab for platform {target} success");
				if( Application.isBatchMode ) {
					Debug.Log("In batch mode prepare to exit");
					EditorApplication.Exit(0);
				}
			});
		}

		[MenuItem("Tools/CI/Rebuild Android Asset Bundle")]
		private static void RebuildAllABAndroid() {
			RebuildAllABForPlatform(BuildTarget.Android);
		}

		[MenuItem("Tools/CI/Rebuild iOS Asset Bundle")]
		private static void RebuildAllABiOS() {
			RebuildAllABForPlatform(BuildTarget.iOS);
		}

		[MenuItem("Tools/CI/Rebuild Windows Asset Bundle")]
		private static void RebuildAllABWindows() {
			RebuildAllABForPlatform(BuildTarget.StandaloneWindows64);
		}

		private static void GenerateXLua() {
			Generator.ClearAll();
			Generator.GenAll();
		}

		private static void CheckMatchParam(ref BuildOptions options, string argToMatch, BuildOptions enumValue) {
			if( Array.IndexOf(Environment.GetCommandLineArgs(), argToMatch) != -1 ) {
				options |= enumValue;
				Debug.Log($"Add Build Option {enumValue}");
			}
		}

		private static BuildOptions AnalyseCommandLineArgs() {
			var options = BuildOptions.None;
			CheckMatchParam(ref options, "dev", BuildOptions.Development);
			CheckMatchParam(ref options, "debug", BuildOptions.AllowDebugging);
			CheckMatchParam(ref options, "scriptOnly", BuildOptions.BuildScriptsOnly);
			CheckMatchParam(ref options, "compress", BuildOptions.CompressWithLz4);
			CheckMatchParam(ref options, "deep", BuildOptions.EnableDeepProfilingSupport);
			CheckMatchParam(ref options, "strict", BuildOptions.StrictMode);
			return options;
		}

		private static string[] CollectScenesPath() {
			var scenesName = new string[EditorBuildSettings.scenes.Length];
			for( int i = 0; i < EditorBuildSettings.scenes.Length; i++ ) {
				var scene = EditorBuildSettings.scenes[i];
				scenesName[i] = scene.path;
			}

			return scenesName;
		}

		private static void ExportPlayerPrepare() {
			Debug.LogWarning($"Command Line : {Environment.CommandLine}");
			Debug.LogWarning($"Current Platform : {EditorUserBuildSettings.activeBuildTarget}");
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

			var buildOptions = new BuildPlayerOptions() {
				options = AnalyseCommandLineArgs(),
				target = BuildTarget.Android,
				locationPathName = buildPath,
				scenes = CollectScenesPath()
			};
			Debug.Log("Gen XLUA Wrap");
			GenerateXLua();
			Debug.Log($"Start Build Player To : {buildPath}");
			BuildPipeline.BuildPlayer(buildOptions);
			Debug.Log("Build Player Finished");

			if( Application.isBatchMode ) {
				EditorApplication.Exit(0);
			}
		}
	}
}