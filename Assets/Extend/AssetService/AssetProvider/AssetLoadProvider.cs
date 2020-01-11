using System;
using UnityEngine;

namespace Extend.AssetService.AssetProvider {
	public abstract class AssetLoadProvider {
		public virtual void Initialize() {
		}

		public virtual string FormatAssetPath(string path) {
			return path.Replace('\\', '/');
		}

		public abstract void ProvideAsync(AssetAsyncLoadHandle loadHandle, Type typ);

		public abstract AssetReference Provide(string path, AssetContainer container, Type typ);

		internal abstract AssetInstance ProvideAssetWithGUID<T>(string guid, AssetContainer container);
		internal abstract string ConvertGUID2Path(string guid);
	}
}