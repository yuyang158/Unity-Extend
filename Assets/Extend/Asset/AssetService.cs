using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Extend.Common;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using XLua;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Extend.Asset {
	[LuaCallCSharp]
	public class AssetService : IService, IServiceUpdate {
		[BlackList]
		public int ServiceType => (int) CSharpServiceManager.ServiceType.ASSET_SERVICE;

		[BlackList]
		public AssetContainer Container { get; } = new();

		private Stopwatch m_stopwatch = new();
		private readonly Stopwatch m_instantiateStopwatch = new();
		public Transform PoolRootNode { get; private set; }

		private readonly Queue<AssetReference.InstantiateAsyncContext> m_deferInstantiates = new(64);

		private float m_singleFrameMaxInstantiateDuration;
		private readonly List<IDisposable> m_disposables = new();

		[BlackList]
		public void AddAfterDestroy(IDisposable disposable) {
			m_disposables.Add(disposable);
		}

		public static AssetService Get() {
			return CSharpServiceManager.Get<AssetService>(CSharpServiceManager.ServiceType.ASSET_SERVICE);
		}

		public void Dump() {
			Container.Dump();
		}

		private GameObject poolGO;

		[BlackList]
		public void AfterSceneLoaded(float maxInstantiateDuration) {
			m_singleFrameMaxInstantiateDuration = maxInstantiateDuration;
			poolGO = new GameObject("Pool");
			Object.DontDestroyOnLoad(poolGO);
			poolGO.SetActive(false);
			PoolRootNode = poolGO.transform;
		}

		[BlackList]
		public void Initialize() {
			m_stopwatch = new Stopwatch();
		}

		public async Task InitAddressable() {
			Debug.Log("addressable is InitializeAsync complete start");
			await Addressables.InitializeAsync().Task;
			Debug.Log("addressable is InitializeAsync complete End...");
		}

		[BlackList]
		public void Destroy() {
			GC.Collect();
			foreach( var disposable in m_disposables ) {
				disposable.Dispose();
			}

			Object.Destroy(poolGO);
			Container.FullCollect();
			Container.Clear();

			Resources.UnloadUnusedAssets();
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
#if ASSET_LOG
				Debug.LogWarning($"Defer instantiate queue : {context}");
#endif
				while( true ) {
					if( !context.Cancel ) {
						if( !context.GetAssetReady() ) {
							break;
						}
#if ASSET_LOG
					Debug.LogWarning($"Async instantiate : {context}");
#endif
						context.Instantiate();
					}

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

		private static void FormatPath(ref string path) {
			if( path.StartsWith("Assets") ) {
				return;
			}

			path = "Assets/Res/" + path;
		}

		public bool Exist(string path) {
			FormatPath(ref path);
			foreach( var locator in Addressables.ResourceLocators ) {
				if( locator.Locate(path, typeof(Object), out _) ) {
					return true;
				}
			}

			return false;
		}

		public AssetAsyncLoadHandle LoadAudioMixerAsync(string path) {
			return LoadAsync<AudioMixer>(path);
		}
		
		public AssetAsyncLoadHandle LoadTextAsync(string path) {
			return LoadAsync<TextAsset>(path);
		}

		public AssetAsyncLoadHandle LoadGameObjectAsync(string path) {
			if( path.Length == 0 ) {
				throw new Exception("Load path is empty.");
			}
			return LoadAsync<GameObject>(path);
		}

		public AssetAsyncLoadHandle LoadShaderAsync(string path) {
			return LoadAsync<Shader>(path);
		}

		public AssetAsyncLoadHandle LoadSpriteAsync(string path) {
			return LoadAsync<Sprite>(path);
		}

		public AssetAsyncLoadHandle LoadSpriteAtlasAsync(string path) {
			return LoadAsync<SpriteAtlas>(path);
		}

		public AssetAsyncLoadHandle LoadMaterialAsync(string path) {
			return LoadAsync<Material>(path);
		}

		public AssetAsyncLoadHandle LoadMeshAsync(string path) {
			return LoadAsync<Mesh>(path);
		}

		public AssetAsyncLoadHandle LoadTextureAsync(string path) {
			return LoadAsync<Texture>(path);
		}

		public AssetAsyncLoadHandle LoadAnimationClipAsync(string path) {
			return LoadAsync<AnimationClip>(path);
		}

		public AssetAsyncLoadHandle LoadAnimatorControllerAsync(string path) {
			return LoadAsync<RuntimeAnimatorController>(path);
		}

		public AssetAsyncLoadHandle LoadScriptableObjectAsync(string path) {
			return LoadAsync<ScriptableObject>(path);
		}

		public AssetAsyncLoadHandle LoadAudioClipAsync(string path) {
			return LoadAsync<AudioClip>(path);
		}

		[BlackList]
		public AssetAsyncLoadHandle LoadAsync<T>(string path) where T : Object {
#if ASSET_LOG
			Debug.LogWarning($"Async load asset : {path}");
#endif
			FormatPath(ref path);
			try {
				var handle = Addressables.LoadAssetAsync<T>(path);
				return new AssetAsyncLoadHandle(Container, handle);
			}
			catch( Exception e ) {
				Debug.LogException(e);
				throw;
			}
		}

		public AssetReference LoadAudioMixer(string path) {
			return Load<AudioMixer>(path);
		}

		public AssetReference LoadGameObject(string path) {
			return Load<GameObject>(path);
		}

		public AssetReference LoadSprite(string path) {
			return Load<Sprite>(path);
		}

		public AssetReference LoadMaterial(string path) {
			return Load<Material>(path);
		}

		public AssetReference LoadMesh(string path) {
			return Load<Mesh>(path);
		}

		public AssetReference LoadTexture(string path) {
			return Load<Texture>(path);
		}

		public AssetReference LoadTextAsset(string path) {
			return Load<TextAsset>(path);
		}

		public AssetReference LoadScriptableObjectAsset(string path) {
			return Load<ScriptableObject>(path);
		}

		public AssetReference LoadUnknownAsset(string path) {
			return Load<Object>(path);
		}

		[BlackList]
		public AssetReference Load<T>(string path) where T : Object {
#if UNITY_DEBUG
			var ticks = m_stopwatch.ElapsedTicks;
			m_stopwatch.Start();
#endif
#if ASSET_LOG
			Debug.LogWarning($"Sync load asset : {path}");
#endif
			FormatPath(ref path);
			var handle = Addressables.LoadAssetAsync<T>(path);
			handle.WaitForCompletion();
			var assetRef = new AssetReference(handle);

#if UNITY_DEBUG
			var time = m_stopwatch.ElapsedTicks - ticks;
			m_stopwatch.Stop();
			var statService = CSharpServiceManager.Get<StatService>(CSharpServiceManager.ServiceType.STAT);
			statService.LogStat("AssetLoad", path, ( time / 10000.0f ).ToString("0.000"));
#endif
			return assetRef;
		}

		public static void Recycle(GameObject go) {
			if( !go ) {
				return;
			}

			var cache = go.GetComponent<AssetServiceManagedGO>();
			if( !cache ) {
#if UNITY_DEBUG
				Debug.LogError($"Recycle a destroy go : {go.name}");
#endif
				return;
			}

			var recyclable = go.GetComponent<IRecyclable>();
			recyclable?.OnRecycle();
			cache.Recycle();
			StatService.Get().Increase(StatService.StatName.IN_USE_GO, -1);
		}

		public static void Recycle(Component component) {
			if( !component ) {
				return;
			}

			Recycle(component.gameObject);
		}

		internal void AddDeferInstantiateContext(AssetReference.InstantiateAsyncContext context) {
#if ASSET_LOG
			Debug.LogWarning($"Enqueue context : {context}");
#endif
			m_deferInstantiates.Enqueue(context);
		}

		public SceneInstance LoadScene(string path, bool additive) {
#if UNITY_DEBUG
			var ticks = m_stopwatch.ElapsedTicks;
			m_stopwatch.Start();
#endif
			var handle = Addressables.LoadSceneAsync(path, additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
			handle.WaitForCompletion();
			var scene = new SceneInstance(handle);
#if UNITY_DEBUG
			var time = m_stopwatch.ElapsedTicks - ticks;
			m_stopwatch.Stop();
			var statService = CSharpServiceManager.Get<StatService>(CSharpServiceManager.ServiceType.STAT);
			statService.LogStat("AssetLoad", path, ( time / 10000.0f ).ToString("0.000"));
#endif
			return scene;
		}

		public void LoadSceneAsync(string path, bool additive, Action<SceneInstance> callback = null) {
			var handle = Addressables.LoadSceneAsync(path, additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
			var sceneInstance = new SceneInstance(handle);
			handle.Completed += _ => { callback.Invoke(sceneInstance); };
		}

		/*public Shader LoadShader(string path) {
			AssetReference shaderRef;
			if( IsAssetBundleMode() ) {
				shaderRef = Load(path, typeof(Shader));
			}
			else {
				shaderRef = Load(path + ".shader", typeof(Shader));
			}
			var shader = shaderRef.GetShader();
			shaderRef.Dispose();
			return shader;
		}*/
	}
}
