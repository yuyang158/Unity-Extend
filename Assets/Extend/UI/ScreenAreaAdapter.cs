using UnityEngine;
#if UNITY_EDITOR && UNITY_IOS
using Extend.UI.Screens;
#endif

namespace Extend.UI {
	
	[ExecuteInEditMode]
	[RequireComponent(typeof(RectTransform))]
	public class ScreenAreaAdapter : MonoBehaviour {
		public bool UpdateEveryFrame = true;

		private RectTransform m_rectTf;
		private Rect m_lastSafeArea;

		private void Awake() {
			m_rectTf = GetComponent<RectTransform>();
			UpdateRect();
		}

		private void Update() {
			if( UpdateEveryFrame || Application.isEditor ) {
				UpdateRect();
			}
		}

		public void UpdateRect() {
#if UNITY_EDITOR && UNITY_IOS
            var safeArea = iOSScreenTypeResolver.Resolve().SafeArea;
#else
			var safeArea = Screen.safeArea;
#endif
			ApplySafeArea(safeArea);
		}

		private void ApplySafeArea(Rect safeArea) {
			if( safeArea == m_lastSafeArea ) return;
			m_rectTf.anchoredPosition = Vector2.zero;
			m_rectTf.sizeDelta = Vector2.zero;

			var anchorMin = safeArea.position;
			var anchorMax = safeArea.position + safeArea.size;
			anchorMin.x /= Screen.width;
			anchorMin.y /= Screen.height;
			anchorMax.x /= Screen.width;
			anchorMax.y /= Screen.height;
			m_rectTf.anchorMin = anchorMin;
			m_rectTf.anchorMax = anchorMax;

			m_lastSafeArea = safeArea;
		}
	}
}