using Extend.AssetService.AssetProvider;
using Extend.Common;
using UnityEngine;

namespace Extend.AssetService {
	public class AssetService : IService, IServiceUpdate {
		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.ASSET_SERVICE;
		public AssetContainer Container { get; } = new AssetContainer();

		private AssetAsyncProvider asyncProvider;
			
		public void Initialize() {
			if( Application.isEditor ) {
				asyncProvider = new ResourcesAsyncProvider();
			}
			else {
				asyncProvider = new AssetBundleAsyncProvider();
			}
		}

		public void Destroy() {
		}

		public void Update() {
			Container.Collect();
		}

		public AssetReference Load(string path) {
			var hash = AssetInstance.GenerateHash(path);
			if( !( Container.TryGetAsset(hash) is AssetInstance asset ) ) {
				asset = new AssetInstance(path);
				Container.Put(asset);
				var unityObject = Resources.Load<Object>(path);
				asset.SetAsset(unityObject, null);
			}
			
			return new AssetReference(asset);
		}

		public AssetAsyncLoadHandle LoadAsync(string path) {
			var handle = new AssetAsyncLoadHandle(Container, asyncProvider, path);
			handle.Execute();
			return handle;
		}
	}
}