using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public class ScalePunchAnimation  : PunchAnimation {
		public override Tweener Active(Transform t) {
			return t.DOPunchScale(Punch, Duration, Vibrato, Elasticity).SetDelay(Delay);
		}
	}
}