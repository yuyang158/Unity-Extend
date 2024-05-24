using System;
using UnityEngine;
using XLua;

namespace Extend.Animation {
	[LuaCallCSharp]
	public abstract class SequencePlayerBase : MonoBehaviour {
		[SerializeField]
		private SequenceAnimator m_animator;

		public SequenceAnimator Animator {
			get => m_animator;
			set {
				m_animator = value;
				if( m_currentAnimation == null && !string.IsNullOrEmpty(m_animator.DefaultAnimation) ) {
					Play(m_animator.DefaultAnimation);
				}
			}
		}

		private float m_timeLast;
		private int m_frame;
		private SequenceAnimator.SequenceAnimation m_currentAnimation;
		private bool m_finished;
		public event Action OnComplete;
		private Component m_spriteContainer;
		private float m_timeScale = 1;

		public virtual void Play(string animationName, int skip = 0, float timeScale = 1) {
			if( !gameObject.activeInHierarchy ) {
				Debug.LogWarning("GameObject is not active", gameObject);
				if( transform.parent ) {
					Debug.LogWarning($"I am {transform.parent.name}.{gameObject.name}");
				}
			}

			if( !m_animator ) {
				Debug.LogWarning($"Animator is empty {name}");
				return;
			}
			
			m_timeLast = skip * 0.0333333f;
			m_frame = skip;
			m_currentAnimation = m_animator.FindAnimation(animationName);
			m_finished = false;
			if( m_currentAnimation == null ) {
				Debug.LogError($"Not found sequence animation : {animationName}, {Animator.name}");
			}
			else {
				m_currentSprite = m_animator.GetSprite(m_animator.name + "_" + m_currentAnimation.Name + m_frame);
			}

			m_timeScale = timeScale;
		}

		public bool Finished => m_finished;
		public int Frame => m_frame;

		public string AnimationName {
			get {
				if( m_currentAnimation == null ) {
					return string.Empty;
				}

				return m_currentAnimation.Name;
			}
		}
		
		private Sprite m_currentSprite;

		private Sprite GetSprite(string spriteName) {
			return m_animator.GetSprite(spriteName);
		}

		protected Sprite CurrentSprite => m_currentSprite;

		protected Sprite CalculateCurrentSprite() {
			if( !m_animator || m_currentAnimation == null ) {
				return null;
			}

			if( m_finished ) {
				return m_currentSprite;
			}

			m_timeLast += Time.deltaTime * m_timeScale;
			if( m_timeLast > 0.033333f ) {
				m_timeLast -= 0.033333f;
				var sprite = GetSprite(m_animator.name + "_" + m_currentAnimation.Name + (m_frame + 1));
				if( !sprite ) {
					m_finished = true;
					OnComplete?.Invoke();

					if( m_currentAnimation.Loop ) {
						m_finished = false;
						m_frame = 0;
						m_currentSprite = GetSprite(m_animator.name + "_" + m_currentAnimation.Name + m_frame);
					}
				}
				else {
					m_currentSprite = sprite;
					m_frame++;
				}
			}

			return m_currentSprite;
		}
	}
}
