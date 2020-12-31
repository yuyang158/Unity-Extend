using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Extend.Asset.AssetProvider;
using Extend.Common;
using UnityEngine;
using XLua;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Extend.Asset {
	[LuaCallCSharp]
	public class AssetService : IService, IServiceUpdate {
		[BlackList]
		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.ASSET_SERVICE;

		[BlackList]
		public AssetContainer Container { get; } = new AssetContainer();

		private AssetLoadProvider m_provider;
		private Stopwatch m_stopwatch = new Stopwatch();
		private readonly Stopwatch m_instantiateStopwatch = new Stopwatch();
		private int m_poolUpdateIndex;
		private Transform m_poolRootNode;
		private readonly List<AssetPool> m_pools = new List<AssetPool>();
		private readonly List<AssetReference.InstantiateAsyncContext> m_deferInstantiates = new List<AssetReference.InstantiateAsyncContext>();

		private readonly bool m_forceAssetBundleMode;
		private float m_singleFrameMaxInstantiateDuration;

		public AssetService(bool forceABMode = false) {
			m_forceAssetBundleMode = forceABMode;
		}

		public static AssetService Get() {
			return CSharpServiceManager.Get<AssetService>(CSharpServiceManager.ServiceType.ASSET_SERVICE);
		}

		public void Dump() {
			Container.Dump();
		}

		[BlackList]
		public void AfterSceneLoaded(float maxInstantiateDuration) {
			m_singleFrameMaxInstantiateDuration = maxInstantiateDuration;
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
			
			var poolGO = new GameObject("Pool");
			Object.DontDestroyOnLoad(poolGO);
			poolGO.SetActive(false);
			m_poolRootNode = poolGO.transform;
		}

		[BlackList]
		public void Destroy() {
		}

		[BlackList]
		public void Update() {
			Container.Collect();

			if( m_deferInstantiates.Count > 0 ) {
				m_instantiateStopwatch.Restart();
				int instantiatedIndex = 0;
				for( var i = 0; i < m_deferInstantiates.Count; i++ ) {
					var context = m_deferInstantiates[i];
					context.Instantiate();
					instantiatedIndex = i;
					if( m_singleFrameMaxInstantiateDuration < m_instantiateStopwatch.Elapsed.TotalSeconds ) {
						break;
					}

				}
				m_deferInstantiates.RemoveRange(0, instantiatedIndex + 1);
			}
			if(m_pools.Count == 0)
				return;

			if( m_poolUpdateIndex >= m_pools.Count ) {
				m_poolUpdateIndex = 0;
			}
			// m_pools[m_poolUpdateIndex]
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

		public static void Recycle(GameObject go) {
			var cache = go.GetComponent<AssetServiceManagedGO>();
			cache.Recycle();
		}
		
		public static void Recycle(Component component) {
			StatService.Get().Increase(StatService.StatName.IN_USE_GO, -1);
			Recycle(component.gameObject);
		}

		internal void AddPool(AssetPool pool) {
			m_pools.Add(pool);
			pool.PoolNode.SetParent(m_poolRootNode, false);
		}

		internal void AddDeferInstantiateContext(AssetReference.InstantiateAsyncContext context) {
			m_deferInstantiates.Add(context);
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
		/// <summary>
		/// 只用于加载场景，注意abName全小写，且不要有文件拓展名
		/// </summary>
		/// <param name="abName">ab名（usage：assets/demos/criwaredemo）</param>
		public void LoadScene(string abName) {
#if UNITY_DEBUG
			var ticks = m_stopwatch.ElapsedTicks;
			m_stopwatch.Start();
#endif
			abName = m_provider.FormatAssetPath(abName);
			m_provider.ProvideScene(abName, Container);
#if UNITY_DEBUG
			var time = m_stopwatch.ElapsedTicks - ticks;
			m_stopwatch.Stop();
			var statService = CSharpServiceManager.Get<StatService>(CSharpServiceManager.ServiceType.STAT);
			statService.LogStat("AssetLoad", abName, time);
#endif
		}
		/// <summary>
		/// 只用于异步加载场景，注意abName全小写，且不要有文件拓展名
		/// </summary>
		/// <param name="abName">路径（usage：assets/demos/criwaredemo）</param>
		public void LoadSceneAsync(string abName)
		{
			var handle = new AssetAsyncLoadHandle(Container, m_provider, abName);
			m_provider.ProvideSceneAsync(handle);
		}
	}
}