using System;
using System.Diagnostics;
using Extend.AssetService.AssetProvider;
using Extend.Common;
using Extend.DebugUtil;
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
		private Stopwatch stopwatch;

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
				// provider = new ResourcesLoadProvider();
				provider = new AssetBundleLoadProvider();
			}
			else {
				provider = new AssetBundleLoadProvider();
			}

			provider.Initialize();
			stopwatch = new Stopwatch();
		}

		[BlackList]
		public void Destroy() {
		}

		[BlackList]
		public void Update() {
			Container.Collect();
		}

		public AssetReference Load(string path, Type typ) {
#if UNITY_DEBUG
			var ticks = stopwatch.ElapsedTicks;
			stopwatch.Start();
#endif
			path = provider.FormatAssetPath(path);
			var assetRef = provider.Provide(path, Container, typ);
#if UNITY_DEBUG
			var time = stopwatch.ElapsedTicks - ticks;
			stopwatch.Stop();
			var statService = CSharpServiceManager.Get<StatService>(CSharpServiceManager.ServiceType.STAT);
			statService.LogStat("AssetLoad", path, time);
#endif
			return assetRef;
		}

		internal AssetInstance LoadAssetWithGUID<T>(string guid) where T : Object {
#if UNITY_DEBUG
			var ticks = stopwatch.ElapsedTicks;
			stopwatch.Start();
#endif
			var assetRef = provider.ProvideAssetWithGUID<T>(guid, Container, out var path);
#if UNITY_DEBUG
			var time = stopwatch.ElapsedTicks - ticks;
			stopwatch.Stop();
			var statService = CSharpServiceManager.Get<StatService>(CSharpServiceManager.ServiceType.STAT);
			statService.LogStat("AssetLoad", path, time);
#endif
			return assetRef;
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