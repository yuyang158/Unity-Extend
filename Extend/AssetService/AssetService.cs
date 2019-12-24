using Extend.AssetService.AssetProvider;
using Extend.Common;
using UnityEngine;
using XLua;

namespace Extend.AssetService {
	[LuaCallCSharp]
	public class AssetService : IService, IServiceUpdate {
		[BlackList]
		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.ASSET_SERVICE;
		[BlackList]
		public AssetContainer Container { get; } = new AssetContainer();
		private AssetLoadProvider provider;

		private readonly bool forceAssetBundleMode;
		public AssetService(bool forceABMode = false) {
			forceAssetBundleMode = forceABMode;
		}

		public static AssetService Get() {
			return CSharpServiceManager.Get<AssetService>(CSharpServiceManager.ServiceType.ASSET_SERVICE);
		}
		
		[BlackList]
		public void Initialize() {
			if( Application.isEditor && forceAssetBundleMode == false ) {
				provider = new ResourcesLoadProvider();
			}
			else {
				provider = new AssetBundleLoadProvider();
			}
			
			provider.Initialize();
		}

		[BlackList]
		public void Destroy() {
		}

		[BlackList]
		public void Update() {
			Container.Collect();
		}

		public AssetReference Load(string path) {
			path = provider.FormatAssetPath(path);
			return provider.Provide(path, Container);
		}
		
		public AssetInstance LoadAsset(string path) {
			path = provider.FormatAssetPath(path);
			return provider.ProvideAsset(path, Container);
		}
		
		public AssetInstance LoadAssetWithGUID(string guid) {
			return provider.ProvideAssetWithGUID(guid, Container);
		}

		public AssetAsyncLoadHandle LoadAsync(string path) {
			var handle = new AssetAsyncLoadHandle(Container, provider, path);
			handle.Execute();
			return handle;
		}
		
		public AssetAsyncLoadHandle LoadAsyncWithGUID(string guid) {
			var handle = new AssetAsyncLoadHandle(Container, provider, guid);
			handle.Execute();
			return handle;
		}

		[BlackList]
		public AssetBundleInstance TryGetAssetBundleInstance(string path) {
			var hash = AssetBundleInstance.GenerateHash(path);
			return Container.TryGetAsset(hash) as AssetBundleInstance;
		}
	}
}