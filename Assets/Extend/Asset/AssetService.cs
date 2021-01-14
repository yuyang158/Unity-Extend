using System;
using System.Collections.Generic;
using System.Diagnostics;
using Extend.Asset.AssetProvider;
using Extend.Common;
using UnityEngine;
using UnityEngine.U2D;
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

			SpriteAtlasManager.atlasRequested += (s, action) => {
				var reference = Load("Assets/SpriteAtlas/" + s, typeof(SpriteAtlas));
				action(reference.GetObject() as SpriteAtlas);
			};
		}

		[BlackList]
		public void Destroy() {
		}

		public void FullCollect() {
			Container.FullCollect();
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

			if( m_pools.Count == 0 )
				return;

			if( m_poolUpdateIndex >= m_pools.Count ) {
				m_poolUpdateIndex = 0;
			}

			// m_pools[m_poolUpdateIndex]
		}

		public bool Exist(string path) {
			return m_provider.Exist(path);
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
			StatService.Get().Increase(StatService.StatName.IN_USE_GO, -1);
		}

		public static void Recycle(Component component) {
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
		/// 只用于加载场景
		/// </summary>
		/// <param name="path">路径名（usage：Assets/Demos/CriWareDemo.unity）</param>
		public void LoadScene(string path) {
#if UNITY_DEBUG
			var ticks = m_stopwatch.ElapsedTicks;
			m_stopwatch.Start();
#endif
			if( !m_provider.ScenePathChecker(path) ) {
				return;
			}

			path = m_provider.FormatScenePath(path);
			m_provider.ProvideScene(path, Container);
#if UNITY_DEBUG
			var time = m_stopwatch.ElapsedTicks - ticks;
			m_stopwatch.Stop();
			var statService = CSharpServiceManager.Get<StatService>(CSharpServiceManager.ServiceType.STAT);
			statService.LogStat("AssetLoad", path, time);
#endif
		}

		/// <summary>
		/// 只用于异步加载场景
		/// </summary>
		/// <param name="path">路径</param>
		public void LoadSceneAsync(string path) {
			if( !m_provider.ScenePathChecker(path) ) {
				return;
			}

			path = m_provider.FormatScenePath(path);
			var handle = new AssetAsyncLoadHandle(Container, m_provider, path);
			m_provider.ProvideSceneAsync(handle);
		}
	}
}