using System;
using UnityEngine;
using UnityEngine.UI;
using XLua;

namespace Extend.Asset {
	[LuaCallCSharp, RequireComponent(typeof(Image))]
	public class ImageSpriteAssetAssignment : SpriteAssetAssignment {
		private Image m_image;
		private void Awake() {
			m_image = GetComponent<Image>();
		}

		protected override void PreLoad() {
			m_image.enabled = false;
		}

		protected override void PostLoad() {
			m_image.enabled = true;
		}

		public override void Apply(Sprite sprite) {
			base.Apply(sprite);
			m_image.sprite = sprite;
		}
	}
}