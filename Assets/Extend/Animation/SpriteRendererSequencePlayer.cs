using UnityEngine;
using XLua;

namespace Extend.Animation {
	[RequireComponent(typeof(SpriteRenderer)), LuaCallCSharp]
	public class SpriteRendererSequencePlayer : SequencePlayerBase {
		private SpriteRenderer m_renderer;
		private void Awake() {
			m_renderer = GetComponent<SpriteRenderer>();
			m_renderer.enabled = false;
			if( Animator ) {
				Animator = Animator;
			}
		}

		public override void Play(string animationName, int skip = 0, float timeScale = 1) {
			base.Play(animationName, skip, timeScale);
			m_renderer.enabled = true;
			m_renderer.sprite = CurrentSprite;
		}

		private void Update() {
			var sprite = CalculateCurrentSprite();
			m_renderer.sprite = sprite;
		}
	}
}
