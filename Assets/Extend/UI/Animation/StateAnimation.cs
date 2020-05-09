using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace Extend.UI.Animation {
	[Serializable]
	public abstract class StateAnimation {
		[SerializeField]
		private float m_delay;
		public float Delay {
			get => m_delay;
			set {
				dirty = true;
				m_delay = value;
			}
		}

		[SerializeField]
		private float m_duration = 1;
		public float Duration {
			get => m_duration;
			set {
				dirty = true;
				m_duration = value;
			}
		}
		
		[SerializeField]
		private Ease m_ease = Ease.Linear;
		public Ease Ease {
			get => m_ease;
			set {
				dirty = true;
				m_ease = value;
			}
		}


		[SerializeField]
		private bool m_active;

		public bool IsActive {
			get => m_active;
			set {
				dirty = true;
				m_active = value;
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