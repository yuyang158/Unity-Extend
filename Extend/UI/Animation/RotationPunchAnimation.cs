using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public class RotationPunchAnimation : PunchAnimation {
		public override Tweener Active(Transform t) {
			return t.DOPunchRotation(Punch, Duration, Vibrato, Elasticity).SetDelay(Delay);
		}
	}
}