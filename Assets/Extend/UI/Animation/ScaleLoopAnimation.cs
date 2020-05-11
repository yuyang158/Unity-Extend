using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public class ScaleLoopAnimation : StateLoopAnimation {
		[SerializeField]
		private Vector3 m_scaleFrom;
		public Vector3 ScaleFrom {
			get => m_scaleFrom;
			set {
				dirty = true;
				m_scaleFrom = value;
			}
		}

		[SerializeField]
		private Vector3 m_scaleTo;
		public Vector3 ScaleTo {
			get => m_scaleTo;
			set {
				m_scaleFrom = value;
				dirty = true;
			}
		}

		protected override Tween DoGenerateTween(RectTransform t, Vector3 start) {
			return t.DOScale(ScaleTo, Duration).SetDelay(Delay).SetEase(Ease).SetLoops(Loops, LoopType).ChangeStartValue(ScaleFrom);
		}
	}
}