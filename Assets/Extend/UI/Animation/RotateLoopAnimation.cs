using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public class RotateLoopAnimation : StateLoopAnimation {
		[SerializeField]
		private Vector3 rotateBy;
		public Vector3 RotateBy {
			get => rotateBy;
			set {
				rotateBy = value;
				dirty = true;
			}
		}

		[SerializeField]
		private RotateMode rotateMode = RotateMode.Fast;
		public RotateMode RotateMode {
			get => rotateMode;
			set {
				rotateMode = value;
				dirty = true;
			}
		}
		protected override Tween DoGenerateTween(RectTransform t, Vector3 start) {
			t.localRotation = Quaternion.Euler(start);
			return t.DOLocalRotate(RotateBy, Duration, RotateMode).SetDelay(Delay).SetEase(Ease).SetLoops(Loops, LoopType);
		}
	}
}