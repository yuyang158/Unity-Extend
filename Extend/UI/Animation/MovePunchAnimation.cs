using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public class MovePunchAnimation : PunchAnimation {
		protected override Tween DoGenerateTween(Transform t) {
			return t.DOPunchPosition(Punch, Duration, Vibrato, Elasticity).SetDelay(Delay);
		}
	}
}