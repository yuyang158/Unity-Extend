using System;
using DG.Tweening;
using Extend.Common;
using UnityEngine;
using UnityEngine.Serialization;

namespace Extend.UI.Animation {
	[Serializable]
	public class FadeInAnimation : StateAnimation {
		[SerializeField, LabelText("Fade From Alpha Value(0-1)")]
		private float m_fadeFrom;
		public float FadeFrom {
			get => m_fadeFrom;
			set {
				dirty = true;
				m_fadeFrom = value;
			}
		}

		private CanvasGroup m_canvasGroup;

		protected override Tween DoGenerateTween(RectTransform t, Vector3 start) {
			if( !m_canvasGroup ) {
				m_canvasGroup = t.GetComponent<CanvasGroup>();
				if( !m_canvasGroup )
					return null;
			}
			return m_canvasGroup.DOFade(start.x, Duration).SetDelay(Delay).SetEase(Ease).ChangeStartValue(FadeFrom);
		}
	}
}