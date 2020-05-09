using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace Extend.UI.Animation {
	[Serializable]
	public class ScaleInAnimation : StateAnimation {
		[SerializeField]
		private Vector3 m_scaleFrom;

		public Vector3 ScaleFrom {
			get => m_scaleFrom;
			set {
				m_scaleFrom = value;
				dirty = true;
			}
		}

		protected override Tween DoGenerateTween(RectTransform t, Vector3 start) {
			return t.DOScale(start, Duration).SetDelay(Delay).SetEase(Ease).ChangeStartValue(ScaleFrom);
		}
	}
}