using System;
using DG.Tweening;
using UnityEngine;

namespace Extend.Tween {
	public interface ITweenValue {
		void Play();

		bool Complete();
	}
	
	[Serializable]
	public abstract class TweenValueBase<T> : ITweenValue {
		[SerializeField]
		protected T m_startValue;

		public T StartValue {
			get => m_startValue;
			set => m_startValue = value;
		}

		[SerializeField]
		protected T m_endValue;

		public T EndValue {
			get => m_endValue;
			set => m_endValue = value;
		}

		[SerializeField]
		private Ease m_ease = Ease.InCirc;

		[SerializeField]
		private float m_delay;

		[SerializeField]
		protected float m_duration = 1;

		[SerializeField]
		private int m_loop = 0;

		[SerializeField]
		private LoopType m_loopType = LoopType.Restart;

		[SerializeField]
		private bool m_editorFold;

		protected abstract void Reset();

		protected abstract T Getter();

		protected abstract void Setter(T val);

		protected abstract Tweener DoPlay();
		private Tweener m_currentPlaying;

		public void Play() {
			Reset();
			m_currentPlaying = DoPlay().SetLoops(m_loop, m_loopType).SetEase(m_ease).SetDelay(m_delay).Play().SetAutoKill(false);
		}

		public bool Complete() {
			return m_currentPlaying != null && m_currentPlaying.IsComplete();
		}
	}
}