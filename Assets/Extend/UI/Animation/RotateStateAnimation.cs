using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public class RotateStateAnimation : StateAnimation {
		[SerializeField]
		private Vector3 rotate;
		public Vector3 Rotate {
			get => rotate;
			set {
				dirty = true;
				rotate = value;
			}
		}

		protected override Tween DoGenerateTween(RectTransform t, Vector3 start) {
			return t.DOLocalRotate(start + Rotate, Duration).SetDelay(Delay).SetEase(Ease);
		}
	}
}