using System;
using System.Collections.Generic;
using Extend.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Extend.Asset {
	public class SpriteAssetService : IService, IServiceUpdate {
		public class SpriteLoadingHandle {
			public static SpriteLoadingHandle Create(SpriteAssetAssignment assignment, AssetAsyncLoadHandle loadHandle, string path) {
				return new SpriteLoadingHandle(loadHandle, assignment, path);
			}
			
			public static SpriteLoadingHandle Create(SpriteAssetAssignment assignment, AssetReference assetRef) {
				return new SpriteLoadingHandle(assetRef, assignment);
			}

			private readonly string m_path;
			private AssetReference m_assetRef;
			private AssetAsyncLoadHandle m_loadHandle;
			private readonly WeakReference<SpriteAssetAssignment> m_assignment;

			private SpriteLoadingHandle(AssetAsyncLoadHandle loadHandle, SpriteAssetAssignment assignment, string path) {
				loadHandle.OnComplete += OnAssetStatusChanged;
				m_path = path;
				m_assignment = new WeakReference<SpriteAssetAssignment>(assignment);
				m_loadHandle = loadHandle;
			}
			
			
			private SpriteLoadingHandle(AssetReference assetRef, SpriteAssetAssignment assignment) {
				m_assetRef = assetRef;
				m_assignment = new WeakReference<SpriteAssetAssignment>(assignment);
				
				TryApplySprite(assignment, m_assetRef);
			}

			private void OnAssetStatusChanged(AssetAsyncLoadHandle loadHandle) {
				m_loadHandle = null;
				if( loadHandle.Asset.Status == AssetRefObject.AssetStatus.DONE ) {
					m_assetRef = loadHandle.Result;
					if( m_assignment.TryGetTarget(out var assignment) ) {
						TryApplySprite(assignment, m_assetRef);
					}
				}
			}

			public void GiveUp() {
				if( m_loadHandle != null ) {
					m_loadHandle.Cancel = true;
				}
				m_assetRef?.Dispose();
			}

			public override string ToString() {
				return $"Loading Sprite --> {m_path}";
			}
		}

		public int ServiceType => (int) CSharpServiceManager.ServiceType.SPRITE_ASSET_SERVICE;

		public static SpriteAssetService Get() {
			return CSharpServiceManager.Get<SpriteAssetService>(CSharpServiceManager.ServiceType.SPRITE_ASSET_SERVICE);
		}

		private readonly Dictionary<int, PackedSprite> m_packedSprites = new Dictionary<int, PackedSprite>();

		private Sprite m_loadingSprite;
		private Material m_rotationMat;

		public PackedSprite.SpriteElement RequestIcon(string path) {
			path = path.Replace('\\', '/');
			string folderName = path.Substring(0, path.IndexOf('/'));

			if( !int.TryParse(folderName, out var size) ) {
				Debug.LogError($"int parse error : {folderName}");
				return null;
			}
			path = "UI/Icon/" + path;

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
			if( sync ) {
				var assetRef = AssetService.Get().Load<Sprite>(path);
				ret = SpriteLoadingHandle.Create(assignment, assetRef);
			}
			else {
				var loadHandle = AssetService.Get().LoadAsync<Sprite>(path);
				ret = SpriteLoadingHandle.Create(assignment, loadHandle, path);
			}

			return ret;
		}

		private static void TryApplySprite(SpriteAssetAssignment assignment, AssetReference assetRef) {
			if( assetRef.IsFinished ) {
				assignment.Apply(assetRef.GetSprite());
			}
		}

		private AssetReference m_iconLoadingRef;
		private AssetReference m_rotateMatRef;

		public void Initialize() {
			// m_packedSprites.Add(128, new PackedSprite(128, false, 2048));

			/*m_iconLoadingRef = AssetService.Get().Load<Sprite>("UI/Common/IconLoading.png");
			m_loadingSprite = m_iconLoadingRef.GetSprite();

			m_rotateMatRef = AssetService.Get().Load<Material>("UI/Common/UIRotate.mat");
			m_rotationMat = m_rotateMatRef.GetMaterial();*/
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
			foreach( var packedSprite in m_packedSprites.Values ) {
				packedSprite.Dispose();
			}

			m_packedSprites.Clear();
			// m_iconLoadingRef.Dispose();
			// m_rotateMatRef.Dispose();
		}

		public void Update() {
		}
	}
}