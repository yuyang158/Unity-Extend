using System;
using DG.Tweening;
using Extend.Common;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public class FadeOutAnimation : StateAnimation {
		[SerializeField, LabelText("Fade From Alpha Value(0-1)")]
		private float m_fadeFrom;

		public float FadeFrom {
			get => m_fadeFrom;
			set {
				m_dirty = true;
				m_fadeFrom = value;
			}
		}

		[SerializeField]
		private bool m_customFromTo;

		[SerializeField]
		private float m_fadeTo;

		private CanvasGroup m_canvasGroup;

		protected override Tween DoGenerateTween(RectTransform t, Vector3 start) {
			if( !m_canvasGroup ) {
				m_canvasGroup = t.GetComponent<CanvasGroup>();
				if( !m_canvasGroup )
					return null;
			}

			return m_customFromTo
				? m_canvasGroup.DOFade(m_fadeTo, Duration).SetDelay(Delay).SetEase(Ease).ChangeStartValue(FadeFrom)
				: m_canvasGroup.DOFade(m_fadeTo, Duration).SetDelay(Delay).SetEase(Ease);
		}
	}
}