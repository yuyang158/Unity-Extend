using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public abstract class PunchAnimation {
		public Vector3 Punch;
		public float Duration = 1;
		public int Vibrato = 10;
		public float Elasticity = 1;
		public float Delay;

		[SerializeField]
		private bool active;
		public bool IsActive => active;

		public abstract Tweener Active(Transform t);
	}
}