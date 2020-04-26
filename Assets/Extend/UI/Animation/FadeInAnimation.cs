using System;
using DG.Tweening;
using Extend.Common;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public class FadeInAnimation : StateAnimation {
		[SerializeField, LabelText("Fade From Alpha Value(0-1)")]
		private float fadeFrom;
		public float FadeFrom {
			get => fadeFrom;
			set {
				dirty = true;
				fadeFrom = value;
			}
		}

		private CanvasGroup canvasGroup;

		protected override Tween DoGenerateTween(RectTransform t, Vector3 start) {
			if( !canvasGroup ) {
				canvasGroup = t.GetComponent<CanvasGroup>();
				if( !canvasGroup )
					return null;
			}
			return canvasGroup.DOFade(start.x, Duration).SetDelay(Delay).SetEase(Ease).ChangeStartValue(FadeFrom);
		}
	}
}