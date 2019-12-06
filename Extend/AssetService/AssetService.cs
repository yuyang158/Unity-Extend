using Extend.Common;

namespace Extend.AssetService {
	public class AssetService : IService, IServiceUpdate {
		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.ASSET_SERVICE;
		private AssetContainer container = new AssetContainer();
		private AssetAsyncProvider _asyncProvider;
			
		public void Initialize() {
			
		}

		public void Destroy() {
		}

		public void Update() {
			container.Collect();
		}

		public AssetReference Load(string path) {
			return null;
		}
		
		
	}
}