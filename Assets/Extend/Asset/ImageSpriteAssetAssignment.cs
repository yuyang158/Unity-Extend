using System;
using UnityEngine;
using UnityEngine.UI;
using XLua;

namespace Extend.Asset {
	[LuaCallCSharp, RequireComponent(typeof(Image))]
	public class ImageSpriteAssetAssignment : SpriteAssetAssignment {
		private Image m_image;

		[SerializeField]
		private bool m_override = true;
		private Image GetImage() {
			if( !m_image ) {
				m_image = GetComponent<Image>();
			}

			return m_image;
		}

		protected override void PreLoad() {
			var img = GetImage();
			if( img.overrideSprite || img.sprite ) {
				return;
			}
			img.enabled = false;
		}

		protected override void PostLoad() {
			GetImage().enabled = true;
		}

		public override void Apply(Sprite sprite) {
			base.Apply(sprite);
			if( m_override ) {
				GetImage().overrideSprite = sprite;
			}
			else {
				GetImage().sprite = sprite;
				GetImage().enabled = sprite != null;
			}
		}
	}
}
