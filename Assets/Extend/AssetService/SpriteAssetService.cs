using System.Collections.Generic;
using Extend.Common;
using UnityEngine;
using UnityEngine.UI;
using XLua;

namespace Extend.AssetService {
	[LuaCallCSharp]
	public class SpriteAssetService : IService, IServiceUpdate {
		private class SpriteLoadingCallback {
			public static void Create(Image img, SpriteRenderer spriteRenderer, AssetInstance asset, string path) {
				new SpriteLoadingCallback(asset, img, spriteRenderer, path);
			}

			private readonly string mPath;
			private readonly AssetInstance mAsset;
			private readonly Image mImg;
			private readonly SpriteRenderer mSpriteRenderer;

			private SpriteLoadingCallback(AssetInstance asset, Image img, SpriteRenderer spriteRenderer, string path) {
				mAsset = asset;
				mPath = path;
				mSpriteRenderer = spriteRenderer;
				mImg = img;

				mAsset.OnStatusChanged += OnAssetStatusChanged;
			}

			private void OnAssetStatusChanged(AssetRefObject _) {
				mAsset.OnStatusChanged -= OnAssetStatusChanged;
				if( mAsset.IsFinished ) {
					TryApplySprite(mImg, mSpriteRenderer, mAsset, mPath);
				}
			}
		}

		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.SPRITE_ASSET_SERVICE;

		public static SpriteAssetService Get() {
			return CSharpServiceManager.Get<SpriteAssetService>(CSharpServiceManager.ServiceType.SPRITE_ASSET_SERVICE);
		}

		private readonly Dictionary<string, AssetInstance> sprites = new Dictionary<string, AssetInstance>();

		public void SetUIImage(Image img, SpriteRenderer spriteRenderer, string path, bool sync = false) {
			if( sprites.TryGetValue(path, out var spriteAsset) ) {
				if( spriteAsset.Status == AssetRefObject.AssetStatus.DESTROYED ) {
					sprites.Remove(path);
				}
				else {
					if( spriteAsset.IsFinished ) {
						TryApplySprite(img, spriteRenderer, spriteAsset, path);
					}
					else {
						SpriteLoadingCallback.Create(img, spriteRenderer, spriteAsset, path);
					}
					return;
				}
			}

			if( sync ) {
				var reference = AssetService.Get().Load(path, typeof(Sprite));
				spriteAsset = reference.Asset;
				TryApplySprite(img, spriteRenderer, spriteAsset, path);
			}
			else {
				var loadHandle = AssetService.Get().LoadAsync(path, typeof(Sprite));
				spriteAsset = loadHandle.Asset;
				SpriteLoadingCallback.Create(img, spriteRenderer, spriteAsset, path);
			}

			sprites.Add(path, spriteAsset);
		}

		public void Release(string key) {
			if( !sprites.TryGetValue(key, out var spriteAsset) ) {
				return;
			}

			spriteAsset.Release();
		}

		private static void TryApplySprite(Image img, SpriteRenderer spriteRenderer, AssetInstance spriteAsset, string spriteKey) {
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

		[BlackList]
		public void Initialize() {
		}

		[BlackList]
		public void Destroy() {
		}

		[BlackList]
		public void Update() {
		}
	}
}