using Extend.AssetService.AssetProvider;
using Extend.Common;
using UnityEngine;

namespace Extend.AssetService {
	public class AssetService : IService, IServiceUpdate {
		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.ASSET_SERVICE;
		public AssetContainer Container { get; } = new AssetContainer();
		
		private AssetLoadProvider provider;
		public void Initialize() {
			if( Application.isEditor ) {
				provider = new ResourcesLoadProvider();
				// provider = new AssetBundleLoadProvider();
			}
			else {
				provider = new AssetBundleLoadProvider();
			}
			
			provider.Initialize();
		}

		public void Destroy() {
		}

		public void Update() {
			Container.Collect();
		}

		public AssetReference Load(string path) {
			path = provider.FormatAssetPath(path);
			return provider.Provide(path, Container);
		}

		public AssetAsyncLoadHandle LoadAsync(string path) {
			var handle = new AssetAsyncLoadHandle(Container, provider, path);
			handle.Execute();
			return handle;
		}
	}
}