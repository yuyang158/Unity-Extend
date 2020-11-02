using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public class RotateOutAnimation : StateAnimation {
		[SerializeField]
		private bool m_customFromTo;
		
		[SerializeField]
		private Vector3 m_rotateFrom;

		public Vector3 RotateFrom {
			get => m_rotateFrom;
			set {
				m_rotateFrom = value;
				m_dirty = true;
			}
		}
		
		[SerializeField]
		private Vector3 m_rotateTo;

		[SerializeField]
		private RotateMode m_rotateMode = RotateMode.Fast;

		public RotateMode RotateMode {
			get => m_rotateMode;
			set {
				m_rotateMode = value;
				m_dirty = true;
			}
		}

		protected override Tween DoGenerateTween(RectTransform t, Vector3 start) {
			return m_customFromTo ? t.DOLocalRotate(m_rotateTo, Duration, RotateMode).SetDelay(Delay).SetEase(Ease).ChangeStartValue(RotateFrom) : 
				t.DOLocalRotate(m_rotateTo, Duration, RotateMode).SetDelay(Delay).SetEase(Ease);
		}
	}
}