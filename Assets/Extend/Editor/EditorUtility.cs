using UnityEditor;
using UnityEngine;

namespace Extend.Editor {
	public static class EditorUtility {
		[MenuItem("Tools/常用工具/打开Log目录")]
		private static void OpenPersistencePath() {
			UnityEditor.EditorUtility.RevealInFinder(Application.persistentDataPath);			
		}
	}
}