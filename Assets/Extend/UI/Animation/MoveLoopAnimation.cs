using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public class MoveLoopAnimation : StateLoopAnimation {
		[SerializeField]
		private Vector3 moveBy;

		protected override Tween DoGenerateTween(RectTransform t, Vector3 start) {
			t.anchoredPosition = start;
			return t.DOAnchorPos(start + moveBy, Duration).SetDelay(Delay).SetEase(Ease).SetLoops(Loops, LoopType);
		}
	}
}