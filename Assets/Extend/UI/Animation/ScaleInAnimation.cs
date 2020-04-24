using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public class ScaleInAnimation : StateAnimation {
		[SerializeField]
		private Vector3 scaleFrom;

		public Vector3 ScaleFrom {
			get => scaleFrom;
			set {
				scaleFrom = value;
				dirty = true;
			}
		}

		protected override Tween DoGenerateTween(RectTransform t, Vector3 start) {
			t.localScale = ScaleFrom;
			return t.DOScale(start, Duration).SetDelay(Delay).SetEase(Ease);
		}
	}
}