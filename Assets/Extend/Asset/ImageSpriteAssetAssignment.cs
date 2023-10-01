using System;
using UnityEngine;
using UnityEngine.UI;
using XLua;

namespace Extend.Asset {
	[LuaCallCSharp, RequireComponent(typeof(Image))]
	public class ImageSpriteAssetAssignment : SpriteAssetAssignment {
		private Image m_image;
		private Image GetImage() {
			if( !m_image ) {
				m_image = GetComponent<Image>();
			}

			return m_image;
		}

		protected override void PreLoad() {
			GetImage().enabled = false;
		}

		protected override void PostLoad() {
			GetImage().enabled = true;
		}

		public override void Apply(Sprite sprite) {
			base.Apply(sprite);
			GetImage().sprite = sprite;
		}
	}
}
