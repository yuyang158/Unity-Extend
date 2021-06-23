using System;
using System.Collections.Generic;
using System.Diagnostics;
using Extend.Asset.AssetProvider;
using Extend.Common;
using UnityEngine;
using UnityEngine.U2D;
using XLua;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Extend.Asset {
	[LuaCallCSharp]
	public class AssetService : IService, IServiceUpdate {
		[BlackList]
		public int ServiceType => (int)CSharpServiceManager.ServiceType.ASSET_SERVICE;

		[BlackList]
		public AssetContainer Container { get; } = new AssetContainer();

		private AssetLoadProvider m_provider;
		private Stopwatch m_stopwatch = new Stopwatch();
		private readonly Stopwatch m_instantiateStopwatch = new Stopwatch();
		public Transform PoolRootNode { get; private set; }

		private readonly Queue<AssetReference.InstantiateAsyncContext> m_deferInstantiates =
			new Queue<AssetReference.InstantiateAsyncContext>(64);

		private readonly bool m_forceAssetBundleMode;
		private float m_singleFrameMaxInstantiateDuration;

		public AssetService(bool forceABMode = false) {
			m_forceAssetBundleMode = forceABMode;
		}

		private readonly List<IDisposable> m_disposables = new List<IDisposable>();

		public void AddAfterDestroy(IDisposable disposable) {
			m_disposables.Add(disposable);
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
#if UNITY_EDITOR
			if( m_forceAssetBundleMode ) {
				m_provider = new AssetBundleLoadProvider();
			}
			else {
				m_provider = new ResourcesLoadProvider();
			}
#else
			m_provider = new AssetBundleLoadProvider();
#endif

			m_provider.Initialize();
			m_stopwatch = new Stopwatch();

			var poolGO = new GameObject("Pool");
			Object.DontDestroyOnLoad(poolGO);
			poolGO.SetActive(false);
			PoolRootNode = poolGO.transform;

			SpriteAtlasManager.atlasRequested += (atlasName, callback) => {
				Debug.LogWarning("Request Atlas : " + atlasName);
				var handle = LoadAsync("Assets/SpriteAtlas/" + atlasName, typeof(SpriteAtlas));
				handle.OnComplete += loadHandle => {
					var reference = loadHandle.Result;
					var atlas = reference.GetObject() as SpriteAtlas;
					if( atlas == null ) {
						Debug.LogError($"Load Asset is : {reference.Asset.UnityObject.GetType()}");
						reference.Dispose();
						return;
					}
					callback(atlas);
				};
			};
		}

		[BlackList]
		public void Destroy() {
			GC.Collect();
			foreach( var disposable in m_disposables ) {
				disposable.Dispose();
			}

			Container.Clear();

			Resources.UnloadUnusedAssets();
			var bundles = AssetBundle.GetAllLoadedAssetBundles();
			foreach( var bundle in bundles ) {
				Debug.LogError($"Unreleased asset bundle : {bundle.name}");
			}

			AssetBundle.UnloadAllAssetBundles(true);
		}

		public void FullCollect() {
			Container.FullCollect();
		}

		[BlackList]
		public void Update() {
			Container.Collect();

			if( m_deferInstantiates.Count > 0 ) {
				m_instantiateStopwatch.Restart();
				var context = m_deferInstantiates.Peek();
				while( true ) {
					if( !context.GetAssetReady() ) {
						break;
					}

					context.Instantiate();
					m_deferInstantiates.Dequeue();
					if( m_deferInstantiates.Count == 0 )
						break;
					context = m_deferInstantiates.Peek();
					if( m_singleFrameMaxInstantiateDuration < m_instantiateStopwatch.Elapsed.TotalSeconds ) {
						break;
					}
				}

				m_instantiateStopwatch.Stop();
			}
		}

		public bool Exist(string path) {
			path = m_provider.FormatAssetPath(path);
			return m_provider.Exist(path);
		}

		public AssetReference Load(string path, Type typ) {
#if UNITY_DEBUG
			var ticks = m_stopwatch.ElapsedTicks;
			m_stopwatch.Start();
#endif
#if ASSET_LOG
			Debug.LogWarning($"Sync Load Asset {path}");
#endif
			path = m_provider.FormatAssetPath(path);
			var assetRef = m_provider.Provide(path, Container, typ);
#if UNITY_DEBUG
			var time = m_stopwatch.ElapsedTicks - ticks;
			m_stopwatch.Stop();
			var statService = CSharpServiceManager.Get<StatService>(CSharpServiceManager.ServiceType.STAT);
			statService.LogStat("AssetLoad", path, ( time / 10000.0f ).ToString("0.000"));
#endif
			return assetRef;
		}

		public static void Recycle(GameObject go) {
			var cache = go.GetComponent<AssetServiceManagedGO>();
#if UNITY_DEBUG
			if( !cache ) {
				Debug.LogError($"Recycle a destroy go : {go.name}");
				return;
			}
#endif
			cache.Recycle();
			StatService.Get().Increase(StatService.StatName.IN_USE_GO, -1);
		}

		public static void Recycle(Component component) {
			Recycle(component.gameObject);
		}

		internal void AddDeferInstantiateContext(AssetReference.InstantiateAsyncContext context) {
			m_deferInstantiates.Enqueue(context);
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
			statService.LogStat("AssetLoad", path, ( time / 10000.0f ).ToString("0.000"));
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
		public void LoadScene(string path, bool add) {
#if UNITY_DEBUG
			var ticks = m_stopwatch.ElapsedTicks;
			m_stopwatch.Start();
#endif
			path = m_provider.FormatScenePath(path);
			m_provider.ProvideScene(path, Container, add);
#if UNITY_DEBUG
			var time = m_stopwatch.ElapsedTicks - ticks;
			m_stopwatch.Stop();
			var statService = CSharpServiceManager.Get<StatService>(CSharpServiceManager.ServiceType.STAT);
			statService.LogStat("AssetLoad", path, ( time / 10000.0f ).ToString("0.000"));
#endif
		}

		/// <summary>
		/// 只用于异步加载场景
		/// </summary>
		/// <param name="path">路径</param>
		public AssetAsyncLoadHandle LoadSceneAsync(string path, bool add) {
			path = m_provider.FormatScenePath(path);
			var handle = new AssetAsyncLoadHandle(Container, m_provider, path);
			m_provider.ProvideSceneAsync(handle, add);
			return handle;
		}
	}
}