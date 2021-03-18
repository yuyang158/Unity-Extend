using System;
using UnityEngine;
using XLua;

namespace Extend.Asset {
	[LuaCallCSharp, RequireComponent(typeof(SpriteRenderer))]
	public class RendererSpriteAssetAssignment : SpriteAssetAssignment {
		private SpriteRenderer m_renderer;
		private void Awake() {
			m_renderer = GetComponent<SpriteRenderer>();
		}

		protected override void PreLoad() {
			m_renderer.enabled = false;
		}

		protected override void PostLoad() {
			m_renderer.enabled = true;
		}

		public override void Apply(Sprite sprite) {
			base.Apply(sprite);
			m_renderer.sprite = sprite;
		}
	}
}