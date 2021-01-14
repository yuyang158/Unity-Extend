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
		
		public override void ActiveAction() {
			m_image.sprite = m_sprite;
		}
	}
}