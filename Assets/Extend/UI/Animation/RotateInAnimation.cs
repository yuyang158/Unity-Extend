using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public class RotateInAnimation : StateAnimation {
		[SerializeField]
		private Vector3 m_rotateFrom;

		public Vector3 RotateFrom {
			get => m_rotateFrom;
			set {
				m_rotateFrom = value;
				dirty = true;
			}
		}

		[SerializeField]
		private RotateMode m_rotateMode = RotateMode.Fast;

		public RotateMode RotateMode {
			get => m_rotateMode;
			set {
				m_rotateMode = value;
				dirty = true;
			}
		}

		protected override Tween DoGenerateTween(RectTransform t, Vector3 start) {
			return t.DOLocalRotate(start, Duration, RotateMode).SetDelay(Delay).SetEase(Ease).ChangeStartValue(RotateFrom);
		}
	}
}