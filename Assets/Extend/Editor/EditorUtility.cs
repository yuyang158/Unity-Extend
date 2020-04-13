using Extend.AssetService.Editor;
using UnityEditor;
using UnityEngine;

namespace Extend.Editor {
	public static class EditorUtility {
		[MenuItem("Tools/常用工具/打开Log目录")]
		private static void OpenPersistencePath() {
			UnityEditor.EditorUtility.RevealInFinder(Application.persistentDataPath);			
		}

		[MenuItem("Tools/CI/Rebuild AB Android")]
		private static void RebuildAllABAndroid() {
			StaticAssetBundleWindow.RebuildAllAssetBundles(BuildTarget.Android);
		}
		
		[MenuItem("Tools/CI/Rebuild AB iOS")]
		private static void RebuildAllABiOS() {
			StaticAssetBundleWindow.RebuildAllAssetBundles(BuildTarget.iOS);
		}
		
		[MenuItem("Tools/CI/Rebuild AB Windows")]
		private static void RebuildAllABWindows() {
			StaticAssetBundleWindow.RebuildAllAssetBundles(BuildTarget.StandaloneWindows64);
		}
	}
}