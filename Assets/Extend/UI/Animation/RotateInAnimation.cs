using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public class RotateInAnimation : StateAnimation {
		[SerializeField]
		private Vector3 rotateFrom;

		public Vector3 RotateFrom {
			get => rotateFrom;
			set {
				rotateFrom = value;
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
			t.rotation = Quaternion.Euler(RotateFrom);
			return t.DOLocalRotate(start, Duration, RotateMode).SetDelay(Delay).SetEase(Ease);
		}
	}
}