using System;
using System.Collections;
using System.IO;
using CSObjectWrapEditor;
using Extend.Asset.Editor;
using Extend.Common.Editor;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Extend.Editor {
	public static class ExtendEditorUtility {
		[MenuItem("Tools/常用工具/打开Log目录")]
		private static void OpenPersistencePath() {
			EditorUtility.RevealInFinder(Application.persistentDataPath);
		}

		private static void RebuildAllABForPlatform(BuildTarget target) {
			Debug.LogWarning($"Rebuild all ab for platform {target}");

			int[] specify = null;
			foreach( var arg in Environment.GetCommandLineArgs() ) {
				if( arg.StartsWith("specify") ) {
					var parts = arg.Split('=');
					if( parts.Length == 1 )
						break;
					if( string.IsNullOrEmpty(parts[1]) )
						break;

					var ids = parts[1].Split(';');
					specify = new int[ids.Length];
					for( int i = 0; i < ids.Length; i++ ) {
						specify[i] = int.Parse(ids[i]);
					}

					break;
				}
			}

			try {
				bool success = specify != null && specify.Length > 0
					? StaticAssetBundleWindow.RebuildSelectedAssetBundles(target, true, specify)
					: StaticAssetBundleWindow.RebuildAllAssetBundles(target, true);
				if( success ) {
					Debug.LogWarning($"Rebuild all ab for platform {target} success");
					if( Application.isBatchMode ) {
						EditorApplication.Exit(0);
					}
				}
				else {
					if( Application.isBatchMode )
						EditorApplication.Exit(1);
				}
			}
			catch( Exception e ) {
				Debug.LogException(e);
				EditorUtility.ClearProgressBar();
				if( Application.isBatchMode ) {
					EditorApplication.Exit(1);
				}
			}
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

			var buildOptions = new BuildPlayerOptions {
				options = AnalyseCommandLineArgs(),
				target = BuildTarget.Android,
				locationPathName = buildPath,
				// scenes = CollectScenesPath()
				scenes = new string[0]
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

		[PostProcessBuild(1)]
		private static void PostProcessIOSProject(BuildTarget target, string path) {
		}

		[MenuItem("Tools/CI/Build iOS Player")]
		private static void ExportIOS() {
			Debug.Log("Start Export iOS Project");
			ExportPlayerPrepare();
			if( EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS ) {
				EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
			}
			EditorUserBuildSettings.iOSBuildConfigType = iOSBuildType.Release;
			var buildPath = Application.dataPath + "/../iOSPlayer";
			if( !Directory.Exists(buildPath) ) {
				Directory.CreateDirectory(buildPath);
			}

			var buildOptions = new BuildPlayerOptions {
				options = AnalyseCommandLineArgs(),
				target = BuildTarget.iOS,
				locationPathName = buildPath,
				// scenes = CollectScenesPath()
				scenes = new string[0]
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
	}
}