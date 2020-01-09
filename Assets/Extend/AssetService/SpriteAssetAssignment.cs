using UnityEngine;
using UnityEngine.UI;
using XLua;

namespace Extend.AssetService {
	[LuaCallCSharp]
	public class SpriteAssetAssignment : MonoBehaviour {
		public bool Sync;
		private string spriteKey;
		private SpriteAssetService.SpriteLoadingHandle loadingHandle;

		public string ImgSpriteKey {
			get => spriteKey;
			set {
				if(spriteKey == value)
					return;
				var img = GetComponent<Image>();
				ApplyNewKey(value, img, null);
			}
		}
		
		public string SpriteRendererKey {
			get => spriteKey;
			set {
				if(spriteKey == value)
					return;
				var spriteRenderer = GetComponent<SpriteRenderer>();
				ApplyNewKey(value, null, spriteRenderer);
			}
		}

		private void ApplyNewKey(string key, Image img, SpriteRenderer spriteRenderer) {
			loadingHandle?.GiveUp();
			if( !string.IsNullOrEmpty(SpriteRendererKey) ) {
				SpriteAssetService.Get().Release(SpriteRendererKey);
			}
			spriteKey = key;
			loadingHandle = SpriteAssetService.Get().SetUIImage(img, spriteRenderer, SpriteRendererKey, Sync);
		}

		private void OnDestroy() {
			loadingHandle?.GiveUp();
			if( string.IsNullOrEmpty(ImgSpriteKey) )
				return;
			SpriteAssetService.Get().Release(ImgSpriteKey);
		}
	}
}