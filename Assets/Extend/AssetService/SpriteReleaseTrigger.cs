using System;
using Extend.Common;
using UnityEngine;
using UnityEngine.UI;
using XLua;

namespace Extend.AssetService {
	[LuaCallCSharp]
	public class SpriteReleaseTrigger : MonoBehaviour {
		[SerializeField]
		private bool sync;
		private string spriteKey;

		public string ImgSpriteKey {
			get => spriteKey;
			set {
				if( !string.IsNullOrEmpty(ImgSpriteKey) ) {
					SpriteAssetService.Get().Release(ImgSpriteKey);
				}

				spriteKey = value;
				var img = GetComponent<Image>();
				SpriteAssetService.Get().SetUIImage(img, null, ImgSpriteKey, sync);
			}
		}
		
		public string SpriteRendererKey {
			get => spriteKey;
			set {
				if( !string.IsNullOrEmpty(SpriteRendererKey) ) {
					SpriteAssetService.Get().Release(SpriteRendererKey);
				}

				spriteKey = value;
				var spriteRenderer = GetComponent<SpriteRenderer>();
				SpriteAssetService.Get().SetUIImage(null, spriteRenderer, SpriteRendererKey, sync);
			}
		}

		private void OnDestroy() {
			if( string.IsNullOrEmpty(ImgSpriteKey) )
				return;
			SpriteAssetService.Get().Release(ImgSpriteKey);
		}
	}
}