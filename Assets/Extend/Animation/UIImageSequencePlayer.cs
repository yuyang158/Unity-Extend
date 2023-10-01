using UnityEngine;
using UnityEngine.UI;
using XLua;

namespace Extend.Animation {
	[RequireComponent(typeof(Image)), LuaCallCSharp]
	public class UIImageSequencePlayer : SequencePlayerBase {
		private Image m_img;
		private void Awake() {
			m_img = GetComponent<Image>();
			if( !Animator ) {
				m_img.enabled = false;
			}
			else {
				Play(Animator.DefaultAnimation);
			}
		}

		public override void Play(string animationName, int skip = 0, float timeScale = 1) {
			base.Play(animationName, skip, timeScale);
			m_img.enabled = true;
		}

		private void Update() {
			var sprite = CalculateCurrentSprite();
			m_img.sprite = sprite;
		}
	}
}
