using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public class FadeStateAnimation : StateAnimation {
		[SerializeField]
		private float m_fade;
		public float Fade {
			get => m_fade;
			set {
				m_dirty = true;
				m_fade = value;
			}
		}

		private CanvasGroup m_canvasGroup;

		protected override Tween DoGenerateTween(RectTransform t, Vector3 start) {
			if( !m_canvasGroup ) {
				m_canvasGroup = t.GetComponent<CanvasGroup>();
				if( !m_canvasGroup )
					return null;
			}
			return m_canvasGroup.DOFade(Fade, Duration).SetDelay(Delay).SetEase(Ease);
		}
	}
}