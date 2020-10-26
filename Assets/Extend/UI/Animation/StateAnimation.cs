using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public abstract class StateAnimation {
		[SerializeField]
		private float m_delay;
		public float Delay {
			get => m_delay;
			set {
				m_dirty = true;
				m_delay = value;
			}
		}

		[SerializeField]
		private float m_duration = 1;
		public float Duration {
			get => m_duration;
			set {
				m_dirty = true;
				m_duration = value;
			}
		}
		
		[SerializeField]
		private Ease m_ease = Ease.Linear;
		public Ease Ease {
			get => m_ease;
			set {
				m_dirty = true;
				m_ease = value;
			}
		}


		[SerializeField]
		private bool m_active;

		public bool IsActive {
			get => m_active;
			set {
				m_dirty = true;
				m_active = value;
			}
		}
		protected bool m_dirty = true;
		protected Tween m_cachedTween;
		
		public Tween Active(RectTransform t, Vector3 start) {
			if( !IsActive ) {
				return null;
			}
			if( m_cachedTween == null || !m_cachedTween.IsActive() || m_dirty || !Application.isPlaying ) {
				m_cachedTween = DoGenerateTween(t, start);
				m_cachedTween.SetAutoKill(false);
				m_dirty = false;
			}
			else {
				m_cachedTween.Restart();
			}

			return m_cachedTween;
		}

		
		protected abstract Tween DoGenerateTween(RectTransform t, Vector3 start);
	}
}