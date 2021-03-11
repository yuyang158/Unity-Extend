using UnityEngine;
using UnityEngine.UI;
using XLua;

namespace Extend.Asset {
	[LuaCallCSharp, RequireComponent(typeof(SpriteRenderer))]
	public class RendererSpriteAssetAssignment : SpriteAssetAssignment {
		private string m_spritePath;
		public string SpritePath {
			get => m_spritePath;
			set {
				m_spritePath = value;
				ApplyNewKey(m_spritePath);
			}
		}
		
		public override void Apply(Sprite sprite) {
			GetComponent<SpriteRenderer>().sprite = sprite;
		}
	}
}