using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public class ScaleOutAnimation : StateAnimation {
		[SerializeField]
		private Vector3 m_scaleFrom;

		public Vector3 ScaleFrom {
			get => m_scaleFrom;
			set {
				m_scaleFrom = value;
				m_dirty = true;
			}
		}

		[SerializeField]
		private bool m_customFromTo;

		[SerializeField]
		private Vector3 m_scaleTo;

		protected override Tween DoGenerateTween(RectTransform t, Vector3 start) {
			return m_customFromTo
				? t.DOScale(m_scaleTo, Duration).SetDelay(Delay).SetEase(Ease).ChangeStartValue(ScaleFrom)
				: t.DOScale(m_scaleTo, Duration).SetDelay(Delay).SetEase(Ease);
		}
	}
}