using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public abstract class PunchAnimation {
		[SerializeField]
		private Vector3 punch;
		[SerializeField]
		public float duration = 1;
		[SerializeField]
		public int vibrato = 10;
		[SerializeField]
		public float elasticity = 1;
		[SerializeField]
		public float delay;

		public Vector3 Punch {
			get => punch;
			set {
				dirty = true;
				punch = value;
			}
		}
		
		public float Duration {
			get => duration;
			set {
				dirty = true;
				duration = value;
			}
		}
		
		public int Vibrato {
			get => vibrato;
			set {
				dirty = true;
				vibrato = value;
			}
		}
		
		public float Elasticity {
			get => elasticity;
			set {
				dirty = true;
				elasticity = value;
			}
		}
		
		public float Delay {
			get => delay;
			set {
				dirty = true;
				delay = value;
			}
		}

		[SerializeField]
		private bool active;
		public bool IsActive => active;
		protected bool dirty = true;
		protected Tween cachedTween;

		public Tween Active(Transform t) {
			if( !active )
				return null;
			if( cachedTween == null || dirty || !Application.isPlaying ) {
				cachedTween = DoGenerateTween(t);
				dirty = false;
			}

			return cachedTween;
		}
		
		protected abstract Tween DoGenerateTween(Transform t);
	}
}