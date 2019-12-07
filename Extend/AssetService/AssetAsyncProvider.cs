namespace Extend.AssetService {
	public abstract class AssetAsyncProvider {
		public virtual void Initialize() {
			
		}
		
		public int GetAssetHashCode(string path) {
			return AssetInstance.GenerateHash(path);
		}

		public abstract void Provide( AssetAsyncLoadHandle loadHandle );
	}
}