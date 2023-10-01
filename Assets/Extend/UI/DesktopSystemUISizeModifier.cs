using System;
using UnityEngine;

namespace Extend.UI {
	public class DesktopSystemUISizeModifier : MonoBehaviour {
		[SerializeField]
		private float m_shrike = 3;

#if UNITY_STANDALONE
		private void Awake() {
			var rectTransform = transform as RectTransform;
			var sizeDelta = rectTransform.sizeDelta;
			rectTransform.sizeDelta = new Vector2(sizeDelta.x / m_shrike, sizeDelta.y / m_shrike);
		}
#endif
	}
}
