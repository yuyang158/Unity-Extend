using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public class RotateLoopAnimation : StateLoopAnimation {
		[SerializeField]
		private Vector3 m_rotateBy;

		public Vector3 RotateBy {
			get => m_rotateBy;
			set {
				m_rotateBy = value;
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
			return t.DOLocalRotate(start + RotateBy, Duration, RotateMode)
				.SetDelay(Delay)
				.SetEase(Ease)
				.SetLoops(Loops, LoopType)
				.ChangeStartValue(start - RotateBy);
		}
	}
}