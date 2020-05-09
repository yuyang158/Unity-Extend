using System.Collections.Generic;
using Extend.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Extend.Asset {
	public class SpriteAssetService : IService, IServiceUpdate {
		public class SpriteLoadingHandle {
			public static SpriteLoadingHandle Create(Image img, SpriteRenderer spriteRenderer, AssetInstance asset, string path) {
				return new SpriteLoadingHandle(asset, img, spriteRenderer, path);
			}

			private readonly string m_path;
			private readonly AssetInstance m_asset;
			private readonly Image m_img;
			private readonly SpriteRenderer m_spriteRenderer;

			private SpriteLoadingHandle(AssetInstance asset, Image img, SpriteRenderer spriteRenderer, string path) {
				m_asset = asset;
				m_path = path;
				m_spriteRenderer = spriteRenderer;
				m_img = img;

				m_asset.OnStatusChanged += OnAssetStatusChanged;
			}

			private void OnAssetStatusChanged(AssetRefObject _) {
				m_asset.OnStatusChanged -= OnAssetStatusChanged;
				if( m_asset.IsFinished ) {
					TryApplySprite(m_img, m_spriteRenderer, m_asset);
				}
			}

			public void GiveUp() {
				m_asset.OnStatusChanged -= OnAssetStatusChanged;
			}

			public override string ToString() {
				return $"Loading Sprite --> {m_path}";
			}
		}

		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.SPRITE_ASSET_SERVICE;

		public static SpriteAssetService Get() {
			return CSharpServiceManager.Get<SpriteAssetService>(CSharpServiceManager.ServiceType.SPRITE_ASSET_SERVICE);
		}

		private readonly Dictionary<string, AssetInstance> m_sprites = new Dictionary<string, AssetInstance>();

		public SpriteLoadingHandle SetUIImage(Image img, SpriteRenderer spriteRenderer, string path, bool sync = false) {
			SpriteLoadingHandle ret = null;
			if( m_sprites.TryGetValue(path, out var spriteAsset) ) {
				if( spriteAsset.Status == AssetRefObject.AssetStatus.DESTROYED ) {
					m_sprites.Remove(path);
				}
				else {
					if( spriteAsset.IsFinished ) {
						TryApplySprite(img, spriteRenderer, spriteAsset);
					}
					else {
						ret = SpriteLoadingHandle.Create(img, spriteRenderer, spriteAsset, path);
					}
					return ret;
				}
			}

			if( sync ) {
				var reference = AssetService.Get().Load(path, typeof(Sprite));
				spriteAsset = reference.Asset;
				TryApplySprite(img, spriteRenderer, spriteAsset);
			}
			else {
				var loadHandle = AssetService.Get().LoadAsync(path, typeof(Sprite));
				spriteAsset = loadHandle.Asset;
				ret = SpriteLoadingHandle.Create(img, spriteRenderer, spriteAsset, path);
			}

			m_sprites.Add(path, spriteAsset);
			return ret;
		}

		public void Release(string key) {
			if( !m_sprites.TryGetValue(key, out var spriteAsset) ) {
				return;
			}

			spriteAsset.Release();
		}

		private static void TryApplySprite(Image img, SpriteRenderer spriteRenderer, AssetInstance spriteAsset) {
			if(!img && !spriteRenderer)
				return;
			
			if( spriteAsset.IsFinished ) {
				spriteAsset.IncRef();
				if(img)
					img.sprite = spriteAsset.UnityObject as Sprite;
				if(spriteRenderer)
					spriteRenderer.sprite = spriteAsset.UnityObject as Sprite;
			}
		}

		public void Initialize() {
		}

		public void Destroy() {
		}

		public void Update() {
		}
	}
}