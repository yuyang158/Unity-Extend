namespace Extend.AssetService.AssetProvider {
	public abstract class AssetLoadProvider {
		public virtual void Initialize() {
		}

		public virtual string FormatAssetPath(string path) {
			return path.Replace('\\', '/');
		}

		public abstract void ProvideAsync(AssetAsyncLoadHandle loadHandle);

		public abstract AssetReference Provide(string path, AssetContainer container);
	}
}