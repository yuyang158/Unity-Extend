using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Extend.Common;
using UnityEngine;
using UnityEngine.UI;

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

		public int ServiceType => (int)CSharpServiceManager.ServiceType.SPRITE_ASSET_SERVICE;

		public static SpriteAssetService Get() {
			return CSharpServiceManager.Get<SpriteAssetService>(CSharpServiceManager.ServiceType.SPRITE_ASSET_SERVICE);
		}

		private readonly Dictionary<string, AssetInstance> m_sprites = new Dictionary<string, AssetInstance>();
		private readonly Dictionary<int, PackedSprite> m_packedSprites = new Dictionary<int, PackedSprite>();


		private Sprite m_loadingSprite;
		private Material m_rotationMat;

		public PackedSprite.SpriteElement RequestIcon(string path) {
			var directoryName = Path.GetDirectoryName(path);
			string folderName;
			if( Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer ) {
				folderName = directoryName.Substring(directoryName.LastIndexOf('\\') + 1);
			}
			else {
				folderName = directoryName.Substring(directoryName.LastIndexOf('/') + 1);
			}

			if( !int.TryParse(folderName, out var size) ) {
				Debug.LogError($"int parse error : {folderName}");
				return null;
			}

#if UNITY_EDITOR
			if( !m_packedSprites.ContainsKey(size) ) {
				Debug.LogError($"Not support texture size : {size}, {path}");
				return null;
			}
#endif

			var packedSprite = m_packedSprites[size];
			return packedSprite.Request(path);
		}

		public SpriteLoadingHandle RequestSprite(SpriteAssetAssignment assignment, string path, bool sync = false) {
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
			if( spriteAsset.Status == AssetRefObject.AssetStatus.DONE ) {
				spriteAsset.IncRef();
				assignment.Apply(spriteAsset.UnityObject as Sprite);
			}
		}

		public void Initialize() {
			m_packedSprites.Add(124, new PackedSprite(124));
			m_packedSprites.Add(252, new PackedSprite(252, false, 2048));

			using( var assetReference = AssetService.Get().Load("UI/Common/IconLoading", typeof(Sprite)) ) {
				m_loadingSprite = assetReference.GetSprite();
			}

			using( var assetReference = AssetService.Get().Load("UI/Common/UIRotate", typeof(Material)) ) {
				m_rotationMat = assetReference.GetMaterial();
			}
		}

		public void ApplyLoadingFx(Image img) {
			img.material = m_rotationMat;
			img.sprite = m_loadingSprite;
		}

		public void ClearLoadingFx(Image img) {
			img.material = null;
			img.sprite = null;
		}

		public void Destroy() {
			foreach( var spriteInstance in m_sprites.Values ) {
				spriteInstance.Destroy();
			}

			m_sprites.Clear();

			foreach( var packedSprite in m_packedSprites.Values ) {
				packedSprite.Dispose();
			}

			m_packedSprites.Clear();
		}

		public void Update() {
		}
	}
}