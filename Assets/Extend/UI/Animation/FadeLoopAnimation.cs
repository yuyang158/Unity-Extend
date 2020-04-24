using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public class FadeLoopAnimation : StateLoopAnimation {
		[SerializeField]
		private float from;
		public float From {
			get => from;
			set {
				dirty = true;
				from = value;
			}
		}

		[SerializeField]
		private float to;
		public float To {
			get => to;
			set {
				to = value;
				dirty = true;
			}
		}


		private CanvasGroup canvasGroup;
		protected override Tween DoGenerateTween(RectTransform t, Vector3 start) {
			if( !canvasGroup ) {
				canvasGroup = t.GetComponent<CanvasGroup>();
			}

			if( !canvasGroup )
				return null;
			canvasGroup.alpha = From;
			return canvasGroup.DOFade(To, Duration).SetDelay(Delay).SetEase(Ease).SetLoops(Loops, LoopType);
		}
	}
}