using UnityEngine;

namespace Extend.UI.Screens {
	public class iOSScreen {
		public readonly Vector2Int ScreenSize;
		public readonly Rect SafeArea;

		public iOSScreen(Vector2Int screenSize, Rect safeArea) {
			ScreenSize = screenSize;
			SafeArea = safeArea;
		}

		public bool IsCurrentScreen() {
			return Screen.width == ScreenSize.x && Screen.height == ScreenSize.y;
		}
	}
}