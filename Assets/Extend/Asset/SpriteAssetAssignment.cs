using Extend.Common.Lua;
using UnityEngine;
using UnityEngine.UI;

namespace Extend.Asset {
	[LuaCallCSharp]
	public class SpriteAssetAssignment : MonoBehaviour {
		public bool Sync;
		private string m_spriteKey;
		private SpriteAssetService.SpriteLoadingHandle m_loadingHandle;

		public string ImgSpriteKey {
			get => m_spriteKey;
			set {
				if(m_spriteKey == value)
					return;
				var img = GetComponent<Image>();
				ApplyNewKey(value, img, null);
			}
		}
		
		public string SpriteRendererKey {
			get => m_spriteKey;
			set {
				if(m_spriteKey == value)
					return;
				var spriteRenderer = GetComponent<SpriteRenderer>();
				ApplyNewKey(value, null, spriteRenderer);
			}
		}

		private void ApplyNewKey(string key, Image img, SpriteRenderer spriteRenderer) {
			m_loadingHandle?.GiveUp();
			if( !string.IsNullOrEmpty(SpriteRendererKey) ) {
				SpriteAssetService.Get().Release(SpriteRendererKey);
			}
			m_spriteKey = key;
			m_loadingHandle = SpriteAssetService.Get().SetUIImage(img, spriteRenderer, SpriteRendererKey, Sync);
		}
		private void OnDestroy() {
			m_loadingHandle?.GiveUp();
			if( string.IsNullOrEmpty(ImgSpriteKey) )
				return;
			SpriteAssetService.Get().Release(ImgSpriteKey);
		}
	}
}