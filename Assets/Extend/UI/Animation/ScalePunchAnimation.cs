using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public class ScalePunchAnimation  : PunchAnimation {
		public ScalePunchAnimation() {
			Punch = Vector3.zero;
		}
		protected override Tween DoGenerateTween(Transform t) {
			return t.DOPunchScale(Punch, Duration, Vibrato, Elasticity).SetDelay(Delay);
		}
	}
}