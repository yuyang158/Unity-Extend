using System;
using System.Collections.Generic;
using Extend.Common;
using UnityEngine;

namespace Extend.Asset {
	public class SpriteAssetService : IService, IServiceUpdate {
		public class SpriteLoadingHandle {
			public static SpriteLoadingHandle Create(SpriteAssetAssignment assignment, AssetInstance asset, string path) {
				return new SpriteLoadingHandle(asset, assignment, path);
			}

			private readonly string m_path;
			private readonly AssetInstance m_asset;
			private readonly WeakReference<SpriteAssetAssignment> m_assignment;

			private SpriteLoadingHandle(AssetInstance asset, SpriteAssetAssignment assignment, string path) {
				m_asset = asset;
				m_path = path;
				m_assignment = new WeakReference<SpriteAssetAssignment>(assignment);
				m_asset.OnStatusChanged += OnAssetStatusChanged;
			}

			private void OnAssetStatusChanged(AssetRefObject _) {
				if( m_asset.IsFinished ) {
					m_asset.OnStatusChanged -= OnAssetStatusChanged;
					if( m_assignment.TryGetTarget(out var assignment) ) {
						TryApplySprite(assignment, m_asset);
					}
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

		public SpriteLoadingHandle SetUIImage(SpriteAssetAssignment assignment, string path, bool sync = false) {
			SpriteLoadingHandle ret = null;
			if( m_sprites.TryGetValue(path, out var spriteAsset) ) {
				if( spriteAsset.Status == AssetRefObject.AssetStatus.DESTROYED ) {
					m_sprites.Remove(path);
				}
				else {
					if( spriteAsset.IsFinished ) {
						TryApplySprite(assignment, spriteAsset);
					}
					else {
						ret = SpriteLoadingHandle.Create(assignment, spriteAsset, path);
					}
					return ret;
				}
			}

			if( sync ) {
				var reference = AssetService.Get().Load(path, typeof(Sprite));
				spriteAsset = reference.Asset;
				TryApplySprite(assignment, spriteAsset);
			}
			else {
				var loadHandle = AssetService.Get().LoadAsync(path, typeof(Sprite));
				spriteAsset = loadHandle.Asset;
				ret = SpriteLoadingHandle.Create(assignment, spriteAsset, path);
			}

			m_sprites.Add(path, spriteAsset);
			return ret;
		}

		public void Release(string key) {
			if( !m_sprites.TryGetValue(key, out var spriteAsset) ) {
				Debug.LogWarning($"Not found sprite key : {key}");
				return;
			}

			spriteAsset.Release();
		}

		private static void TryApplySprite(SpriteAssetAssignment assignment, AssetInstance spriteAsset) {
			if( spriteAsset.IsFinished ) {
				spriteAsset.IncRef();
				assignment.Apply(spriteAsset.UnityObject as Sprite);
			}
		}

		public void Initialize() {
		}

		public void Destroy() {
			foreach( var spriteInstance in m_sprites.Values ) {
				spriteInstance.Destroy();
			}
			m_sprites.Clear();
		}

		public void Update() {
		}
	}
}