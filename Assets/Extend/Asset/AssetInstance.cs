using Extend.Common;
using UnityEngine;

namespace Extend.Asset {
	public class AssetInstance : AssetRefObject {
		public Object UnityObject { get; private set; }
		private AssetBundleInstance RefAssetBundle { get; set; }
		public string AssetPath { get; }

		public float CreateTime;

		public AssetInstance(string assetPath) {
			if( string.IsNullOrEmpty(assetPath) ) {
				Debug.LogError("Asset path is empty");
				return;
			}
			AssetPath = string.Intern(assetPath);
			AssetService.Get().Container.Put(this);
		}

		public virtual void SetAsset(Object unityObj, AssetBundleInstance refAssetBundle) {
			UnityObject = unityObj;
			if( refAssetBundle != null ) {
				RefAssetBundle = refAssetBundle;
				RefAssetBundle.IncRef();
			}
			Status = UnityObject ? AssetStatus.DONE : AssetStatus.FAIL;
			if( Status == AssetStatus.DONE ) {
				StatService.Get().Increase(StatService.StatName.ASSET_COUNT, 1);
				CreateTime = Time.realtimeSinceStartup;
#if UNITY_DEBUG
				var service = CSharpServiceManager.Get<AssetFullStatService>(CSharpServiceManager.ServiceType.ASSET_FULL_STAT);
				service.OnAssetLoaded(this);
#endif
			}
		}

		public override void Destroy() {
			if( Status == AssetStatus.DONE ) {
				StatService.Get().Increase(StatService.StatName.ASSET_COUNT, -1);
#if UNITY_DEBUG
				var service = CSharpServiceManager.Get<AssetFullStatService>(CSharpServiceManager.ServiceType.ASSET_FULL_STAT);
				service.OnAssetUnloaded(this);
#endif
			}
			Status = AssetStatus.DESTROYED;
			UnityObject = null;
			RefAssetBundle?.Release();
			RefAssetBundle = null;
		}
		
		public static int GenerateHash(string path) {
			return path.GetHashCode();
		}

		public override int GetHashCode() {
			return GenerateHash(AssetPath);
		}

		public override string ToString() {
			return AssetPath;
		}
	}
}