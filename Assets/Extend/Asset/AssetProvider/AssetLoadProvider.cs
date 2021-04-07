using System;
using UnityEngine;

namespace Extend.Asset.AssetProvider {
	public abstract class AssetLoadProvider {
		public abstract void Initialize();

		public virtual string FormatAssetPath(string path) {
			return path.Replace('\\', '/');
		}

		public virtual string FormatScenePath(string path)
		{
			return path.Replace('\\', '/');
		}

		public abstract void ProvideAsync(AssetAsyncLoadHandle loadHandle, Type typ);

		public abstract AssetReference Provide(string path, AssetContainer container, Type typ);

		public abstract void ProvideSceneAsync(AssetAsyncLoadHandle loadHandle, bool add);

		public abstract void ProvideScene(string path, AssetContainer container, bool add);

		public abstract bool Exist(string path);

		internal abstract AssetInstance ProvideAssetWithGUID<T>(string guid, AssetContainer container, out string path);
		internal abstract string ConvertGUID2Path(string guid);
	}
}