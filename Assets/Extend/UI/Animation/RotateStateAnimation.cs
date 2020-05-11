using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public class RotateStateAnimation : StateAnimation {
		[SerializeField]
		private Vector3 m_rotate;
		public Vector3 Rotate {
			get => m_rotate;
			set {
				dirty = true;
				m_rotate = value;
			}
		}

		protected override Tween DoGenerateTween(RectTransform t, Vector3 start) {
			return t.DOLocalRotate(start + Rotate, Duration).SetDelay(Delay).SetEase(Ease);
		}
	}
}