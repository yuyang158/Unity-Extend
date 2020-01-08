using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public class RotationStateAnimation : StateAnimation {
		[SerializeField]
		private Vector3 rotation;
		public Vector3 Rotation {
			get => rotation;
			set {
				dirty = true;
				rotation = value;
			}
		}

		protected override Tween DoGenerateTween(RectTransform t, Vector3 start) {
			return t.DOLocalRotate(start + Rotation, Duration).SetDelay(Delay).SetEase(Ease);
		}
	}
}