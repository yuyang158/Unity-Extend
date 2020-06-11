using UnityEngine;
using UnityEngine.UI;
using XLua;

namespace Extend.Asset {
	[LuaCallCSharp]
	public class SpriteAssetAssignment : MonoBehaviour {
		public bool Sync;
		private string m_spriteKey;
		private SpriteAssetService.SpriteLoadingHandle m_loadingHandle;

		public string ImgSpriteKey {
			get => m_spriteKey;
			set {
				if( m_spriteKey == value )
					return;
				var img = GetComponent<Image>();
				ApplyNewKey(value, img, null);
			}
		}

		public string SpriteRendererKey {
			get => m_spriteKey;
			set {
				if( m_spriteKey == value )
					return;
				var spriteRenderer = GetComponent<SpriteRenderer>();
				ApplyNewKey(value, null, spriteRenderer);
			}
		}

		private void ApplyNewKey(string key, Image img, SpriteRenderer spriteRenderer) {
			m_loadingHandle?.GiveUp();
			m_loadingHandle = null;
			if( !string.IsNullOrEmpty(SpriteRendererKey) ) {
				SpriteAssetService.Get().Release(SpriteRendererKey);
			}

			m_spriteKey = key;
			if( string.IsNullOrEmpty(m_spriteKey) ) {
				if( img )
					img.sprite = null;
				if( spriteRenderer )
					spriteRenderer.sprite = null;
			}
			else {
				m_loadingHandle = SpriteAssetService.Get().SetUIImage(img, spriteRenderer, SpriteRendererKey, Sync);
			}
		}

		private void OnDestroy() {
			m_loadingHandle?.GiveUp();
			if( string.IsNullOrEmpty(ImgSpriteKey) )
				return;
			SpriteAssetService.Get().Release(ImgSpriteKey);
		}
	}
}