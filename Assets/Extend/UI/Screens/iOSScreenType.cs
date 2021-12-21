using System.Collections.Generic;
using UnityEngine;

namespace Extend.UI.Screens {
	public static class iOSScreenType {
		// iPhone X/XS/11 Pro Portrait
		public static readonly iOSScreen iPhoneXPortrait = new(
			new Vector2Int(1125, 2436),
			new Rect(0, 102, 1125, 2202)
		);

		// iPhone X/XS/11 Pro Landscape
		public static readonly iOSScreen iPhoneXLandscape = new(
			new Vector2Int(2436, 1125),
			new Rect(132, 63, 2172, 1062)
		);

		// iPhone XR/11 Portrait
		public static readonly iOSScreen iPhoneXRPortrait = new(
			new Vector2Int(828, 1792),
			new Rect(0, 68, 828, 1636)
		);

		// iPhone XR/11 Landscape
		public static readonly iOSScreen iPhoneXRLandscape = new(
			new Vector2Int(1792, 828),
			new Rect(88, 42, 1616, 786)
		);

		// iPhone XS Max/11 Pro Max Portrait
		public static readonly iOSScreen iPhoneXSMaxPortrait = new(
			new Vector2Int(1242, 2688),
			new Rect(0, 102, 1242, 2454)
		);

		// iPhone XS Max/11 Pro Max Landscape
		public static readonly iOSScreen iPhoneXSMaxLandscape = new(
			new Vector2Int(2688, 1242),
			new Rect(132, 63, 2454, 1242)
		);

		public static iOSScreen Unknown() => new(
			new Vector2Int(Screen.width, Screen.height),
			Screen.safeArea
		);

		public static IEnumerable<iOSScreen> Values {
			get {
				yield return iPhoneXPortrait;
				yield return iPhoneXLandscape;
				yield return iPhoneXRPortrait;
				yield return iPhoneXRLandscape;
				yield return iPhoneXSMaxPortrait;
				yield return iPhoneXSMaxLandscape;
			}
		}
	}
}