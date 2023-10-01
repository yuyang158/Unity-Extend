using Extend.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Extend.UI {
	[RequireComponent(typeof(CanvasScaler)), ExecuteAlways]
	public class ScreenAspectMatch : MonoBehaviour {
		private const float MinAspect = 9.0f / 16.0f;
		private const float MaxAspect = 3.0f / 4.0f;

		private void Awake() {
			var scaler = GetComponent<CanvasScaler>();

			if( Application.isMobilePlatform ) {
				if( Screen.orientation == ScreenOrientation.Portrait ) {
					scaler.matchWidthOrHeight = Mathf.Clamp01(( Screen.width / (float)Screen.height - MinAspect ) / ( MaxAspect - MinAspect ));
				}
				else {
					scaler.matchWidthOrHeight = 1 - Mathf.Clamp01(( Screen.width / (float)Screen.height - MinAspect ) / ( MaxAspect - MinAspect ));
				}
			}
			else {
				scaler.matchWidthOrHeight = 1 - Mathf.Clamp01(( Screen.width / (float)Screen.height - MinAspect ) / ( MaxAspect - MinAspect ));
			}
		}
	}
}