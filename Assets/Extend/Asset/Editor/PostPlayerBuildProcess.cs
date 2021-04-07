using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace Extend.Asset.Editor {
	public static class PostPlayerBuildProcess {
		[PostProcessBuildAttribute(2)]
		private static void OnIOSPlayerBuilt(BuildTarget target, string pathToBuiltProject) {
#if UNITY_IOS
			var infoPlistPath = $"{pathToBuiltProject}/Info.plist";
			var plist = new PlistDocument();
			plist.ReadFromFile(infoPlistPath);
			plist.root.SetBoolean("UIFileSharingEnabled", true);
			
			plist.WriteToFile(infoPlistPath);
#endif
		}
	}
}