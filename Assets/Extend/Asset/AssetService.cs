using System;
using System.Diagnostics;
using Extend.Asset.AssetProvider;
using Extend.Common;
using UnityEngine;
using XLua;
using Object = UnityEngine.Object;

namespace Extend.Asset {
	[LuaCallCSharp]
	public class AssetService : IService, IServiceUpdate {
		[BlackList]
		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.ASSET_SERVICE;

		[BlackList]
		public AssetContainer Container { get; } = new AssetContainer();

		private AssetLoadProvider m_provider;
		private Stopwatch m_stopwatch;

		private readonly bool m_forceAssetBundleMode;

		public AssetService(bool forceABMode = false) {
			m_forceAssetBundleMode = forceABMode;
		}

		public static AssetService Get() {
			return CSharpServiceManager.Get<AssetService>(CSharpServiceManager.ServiceType.ASSET_SERVICE);
		}

		[BlackList]
		public void Initialize() {
			if( Application.isEditor && m_forceAssetBundleMode == false ) {
				m_provider = new ResourcesLoadProvider();
				// provider = new AssetBundleLoadProvider();
			}
			else {
				m_provider = new AssetBundleLoadProvider();
			}

			m_provider.Initialize();
			m_stopwatch = new Stopwatch();
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
			var ticks = m_stopwatch.ElapsedTicks;
			m_stopwatch.Start();
#endif
			path = m_provider.FormatAssetPath(path);
			var assetRef = m_provider.Provide(path, Container, typ);
#if UNITY_DEBUG
			var time = m_stopwatch.ElapsedTicks - ticks;
			m_stopwatch.Stop();
			var statService = CSharpServiceManager.Get<StatService>(CSharpServiceManager.ServiceType.STAT);
			statService.LogStat("AssetLoad", path, time);
#endif
			return assetRef;
		}

		internal AssetInstance LoadAssetWithGUID<T>(string guid) where T : Object {
#if UNITY_DEBUG
			var ticks = m_stopwatch.ElapsedTicks;
			m_stopwatch.Start();
#endif
			var assetRef = m_provider.ProvideAssetWithGUID<T>(guid, Container, out var path);
#if UNITY_DEBUG
			var time = m_stopwatch.ElapsedTicks - ticks;
			m_stopwatch.Stop();
			var statService = CSharpServiceManager.Get<StatService>(CSharpServiceManager.ServiceType.STAT);
			statService.LogStat("AssetLoad", path, time);
#endif
			return assetRef;
		}

		public AssetAsyncLoadHandle LoadAsync(string path, Type typ) {
			var handle = new AssetAsyncLoadHandle(Container, m_provider, path);
			handle.Execute(typ);
			return handle;
		}

		internal AssetAsyncLoadHandle LoadAsyncWithGUID(string guid, Type typ) {
			var path = m_provider.ConvertGUID2Path(guid);
			return LoadAsync(path, typ);
		}
	}
}