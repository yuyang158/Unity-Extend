using System;
using UnityEngine;
using UnityEngine.UI;

namespace Extend.Switcher.Action {
	[Serializable, UnityEngine.Scripting.Preserve]
	public class ImageSwitcherAction : SwitcherAction {
		[SerializeField]
		private Image m_image;

		[SerializeField]
		private Sprite m_sprite;

		private Sprite m_originSprite;
		
		public override void ActiveAction() {
			m_originSprite = m_image.sprite;
			m_image.sprite = m_sprite;
		}

		public override void DeactiveAction() {
			m_image.sprite = m_originSprite;
		}
	}
}