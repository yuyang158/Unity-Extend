using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public class FadeLoopAnimation : StateLoopAnimation {
		[SerializeField]
		private float m_from;
		public float From {
			get => m_from;
			set {
				m_dirty = true;
				m_from = value;
			}
		}

		[SerializeField]
		private float m_to;
		public float To {
			get => m_to;
			set {
				m_to = value;
				m_dirty = true;
			}
		}


		private CanvasGroup canvasGroup;
		protected override Tween DoGenerateTween(RectTransform t, Vector3 start) {
			if( !canvasGroup ) {
				canvasGroup = t.GetComponent<CanvasGroup>();
			}

			return canvasGroup ? canvasGroup.DOFade(To, Duration).SetDelay(Delay).SetEase(Ease).SetLoops(Loops, LoopType).ChangeStartValue(From) : null;
		}
	}
}