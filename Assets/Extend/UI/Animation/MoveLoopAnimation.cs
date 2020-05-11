using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public class MoveLoopAnimation : StateLoopAnimation {
		[SerializeField]
		private Vector3 m_moveBy;

		protected override Tween DoGenerateTween(RectTransform t, Vector3 start) {
			return t.DOAnchorPos(start + m_moveBy, Duration).SetDelay(Delay).SetEase(Ease).SetLoops(Loops, LoopType).ChangeStartValue(start - m_moveBy);
		}
	}
}