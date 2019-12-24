using UnityEngine;

namespace Extend.AssetService.AssetProvider {
	public abstract class AssetLoadProvider {
		public virtual void Initialize() {
		}

		public virtual string FormatAssetPath(string path) {
			return path.Replace('\\', '/');
		}

		public abstract void ProvideAsync(AssetAsyncLoadHandle loadHandle);

		public abstract AssetReference Provide(string path, AssetContainer container);

		internal abstract AssetInstance ProvideAsset(string path, AssetContainer container);
		internal abstract AssetInstance ProvideAssetWithGUID(string guid, AssetContainer container);
		internal abstract string ConvertGUID2Path(string guid);
	}
}