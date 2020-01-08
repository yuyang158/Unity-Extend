using System;
using Extend.AssetService.AssetProvider;
using Extend.Common;
using UnityEngine;
using XLua;
using Object = UnityEngine.Object;

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

		public AssetReference Load(string path, Type typ) {
			path = provider.FormatAssetPath(path);
			return provider.Provide(path, Container, typ);
		}
		
		internal AssetInstance LoadAssetWithGUID<T>(string guid) where T : Object {
			return provider.ProvideAssetWithGUID<T>(guid, Container);
		}

		public AssetAsyncLoadHandle LoadAsync(string path, Type typ) {
			var handle = new AssetAsyncLoadHandle(Container, provider, path);
			handle.Execute(typ);
			return handle;
		}
		
		internal AssetAsyncLoadHandle LoadAsyncWithGUID(string guid, Type typ) {
			var path = provider.ConvertGUID2Path(guid);
			return LoadAsync(path, typ);
		}
	}
}