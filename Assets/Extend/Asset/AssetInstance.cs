using Extend.Common;
using UnityEngine;

namespace Extend.Asset {
	public class AssetInstance : AssetRefObject {
		public Object UnityObject { get; private set; }
		private AssetBundleInstance RefAssetBundle { get; set; }
		public string AssetPath { get; }

		public AssetInstance(string assetPath) {
			AssetPath = string.Intern(assetPath);
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
			}
		}

		public override void Destroy() {
			if( Status == AssetStatus.DONE ) {
				StatService.Get().Increase(StatService.StatName.ASSET_COUNT, -1);
			}
			Status = AssetStatus.DESTROYED;
			RefAssetBundle?.Release();
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