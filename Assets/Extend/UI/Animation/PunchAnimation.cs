using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace Extend.UI.Animation {
	[Serializable]
	public abstract class PunchAnimation {
		[SerializeField]
		private Vector3 m_punch;
		[SerializeField]
		public float m_duration = 1;
		[SerializeField]
		public int m_vibrato = 10;
		[SerializeField]
		public float m_elasticity = 1;
		[SerializeField]
		public float m_delay;

		public Vector3 Punch {
			get => m_punch;
			set {
				m_dirty = true;
				m_punch = value;
			}
		}
		
		public float Duration {
			get => m_duration;
			set {
				m_dirty = true;
				m_duration = value;
			}
		}
		
		public int Vibrato {
			get => m_vibrato;
			set {
				m_dirty = true;
				m_vibrato = value;
			}
		}
		
		public float Elasticity {
			get => m_elasticity;
			set {
				m_dirty = true;
				m_elasticity = value;
			}
		}
		
		public float Delay {
			get => m_delay;
			set {
				m_dirty = true;
				m_delay = value;
			}
		}

		[SerializeField]
		private bool m_active;
		protected bool m_dirty = true;
		protected Tween m_cachedTween;

		public Tween Active(Transform t) {
			if( !m_active )
				return null;
			if( m_cachedTween == null || m_dirty || !Application.isPlaying ) {
				m_cachedTween = DoGenerateTween(t);
				m_dirty = false;
			}

			return m_cachedTween;
		}
		
		protected abstract Tween DoGenerateTween(Transform t);
	}
}