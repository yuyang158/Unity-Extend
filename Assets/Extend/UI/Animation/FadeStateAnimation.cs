using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public class FadeStateAnimation : StateAnimation {
		[SerializeField]
		private float fade;
		public float Fade {
			get => fade;
			set {
				dirty = true;
				fade = value;
			}
		}

		private CanvasGroup canvasGroup;

		protected override Tween DoGenerateTween(RectTransform t, Vector3 start) {
			if( !canvasGroup ) {
				canvasGroup = t.GetComponent<CanvasGroup>();
				if( !canvasGroup )
					return null;
			}
			return canvasGroup.DOFade(Fade, Duration).SetDelay(Delay).SetEase(Ease);
		}
	}
}