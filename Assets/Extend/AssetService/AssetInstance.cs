using Extend.Common;
using UnityEngine;

namespace Extend.AssetService {
	public class AssetInstance : AssetRefObject {
		public Object UnityObject { get; private set; }
		private AssetBundleInstance RefAssetBundle { get; set; }
		private string AssetPath { get; }

		public AssetInstance(string assetPath) {
			AssetPath = string.Intern(assetPath);
		}

		public void SetAsset(Object unityObj, AssetBundleInstance refAssetBundle) {
			UnityObject = unityObj;
			if( refAssetBundle != null ) {
				RefAssetBundle = refAssetBundle;
				RefAssetBundle.IncRef();
			}
			Status = UnityObject ? AssetStatus.DONE : AssetStatus.FAIL;
		}

		public override void Destroy() {
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