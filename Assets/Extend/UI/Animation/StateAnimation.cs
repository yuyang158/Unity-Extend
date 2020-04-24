using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public abstract class StateAnimation {
		[SerializeField]
		private float delay;
		public float Delay {
			get => delay;
			set {
				dirty = true;
				delay = value;
			}
		}

		[SerializeField]
		private float duration = 1;
		public float Duration {
			get => duration;
			set {
				dirty = true;
				duration = value;
			}
		}
		
		[SerializeField]
		private Ease ease = Ease.Linear;
		public Ease Ease {
			get => ease;
			set {
				dirty = true;
				ease = value;
			}
		}


		[SerializeField]
		private bool active;

		public bool IsActive {
			get => active;
			set {
				dirty = true;
				active = value;
			}
		}
		protected bool dirty = true;
		protected Tween cachedTween;
		
		public Tween Active(RectTransform t, Vector3 start) {
			if( !IsActive ) {
				return null;
			}
			if( cachedTween == null || dirty || !Application.isPlaying ) {
				cachedTween = DoGenerateTween(t, start);
				dirty = false;
			}

			return cachedTween;
		}

		
		protected abstract Tween DoGenerateTween(RectTransform t, Vector3 start);
	}
}