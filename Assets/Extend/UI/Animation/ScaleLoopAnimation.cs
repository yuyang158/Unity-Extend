using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public class ScaleLoopAnimation : StateLoopAnimation {
		[SerializeField]
		private Vector3 scaleFrom;
		public Vector3 ScaleFrom {
			get => scaleFrom;
			set {
				dirty = true;
				scaleFrom = value;
			}
		}

		[SerializeField]
		private Vector3 scaleTo;
		public Vector3 ScaleTo {
			get => scaleTo;
			set {
				scaleFrom = value;
				dirty = true;
			}
		}


		protected override Tween DoGenerateTween(RectTransform t, Vector3 start) {
			t.localScale = ScaleFrom;
			return t.DOScale(ScaleTo, Duration).SetDelay(Delay).SetEase(Ease).SetLoops(Loops, LoopType);
		}
	}
}