using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public class MovePunchAnimation : PunchAnimation {
		public override Tweener Active(Transform t) {
			return t.DOPunchPosition(Punch, Duration, Vibrato, Elasticity).SetDelay(Delay);
		}
	}
}