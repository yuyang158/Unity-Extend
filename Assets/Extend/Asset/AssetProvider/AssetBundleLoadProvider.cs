using System;
using System.Collections.Generic;
using System.IO;
using Extend.Asset.AssetOperator;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

#if UNITY_ANDROID
using System.Threading;
using UnityEngine.Networking;
#endif

namespace Extend.Asset.AssetProvider {
	public class AssetBundleLoadProvider : AssetLoadProvider {
		private struct AssetPath {
			public string Path;
			public string ABName;
		}

		private AssetBundleManifest m_manifest;
		private static string STREAMING_ASSET_PATH;
		private static string PERSISTENT_DATA_PATH;
		private readonly Dictionary<string, AssetPath> m_asset2ABMap = new Dictionary<string, AssetPath>();
		private readonly Dictionary<string, string> m_guid2AssetPath = new Dictionary<string, string>();

		public override string FormatAssetPath(string path)
		{
			path = base.FormatAssetPath(path).ToLower();
			if( path.StartsWith("assets") ) {
				return path;
			}

			return "assets/resources/" + path;
		}

		public override string FormatScenePath(string path)
		{
			path = base.FormatScenePath(path).ToLower().Replace(".unity", "");
			if( path.StartsWith("assets") ) {
				return path;
			}
			return "assets/resources/" + path;
		}

		public override void Initialize() {
#if UNITY_IOS
			const string platform = "iOS";
#elif UNITY_ANDROID
			const string platform = "Android";
#else
			const string platform = "StandaloneWindows64";
#endif
			STREAMING_ASSET_PATH = Path.Combine(Application.streamingAssetsPath, "ABBuild", platform);
			PERSISTENT_DATA_PATH = Path.Combine(Application.persistentDataPath, "ABBuild", platform);

			var manifestPath = DetermineLocation(platform);
			var manifestAB = AssetBundle.LoadFromFile(manifestPath);
			m_manifest = manifestAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

#if UNITY_ANDROID
			var mapPath = DetermineLocation("package.conf", out var persistent);
			TextReader reader;
			if( persistent ) {
				reader = new StreamReader(mapPath);
			}
			else {
				var uwr = UnityWebRequest.Get(mapPath);
				uwr.SendWebRequest();
				while( !uwr.isDone ) {
					Thread.Sleep(1);
				}
				reader = new StringReader(uwr.downloadHandler.text);
				uwr.Dispose();
			}
			
			using( reader ) {
#else
			var mapPath = DetermineLocation("package.conf");
			using( var reader = new StreamReader(mapPath) ) {
#endif
				var line = reader.ReadLine();
				while( !string.IsNullOrEmpty(line) ) {
					var segments = line.Split('|');
					Assert.IsTrue(segments.Length >= 3);
					var fullPath = segments[0];
					var extension = Path.GetExtension(fullPath);
					if( string.IsNullOrEmpty(extension) ) {
						line = reader.ReadLine();
						continue;
					}
					fullPath = fullPath.Substring(0, fullPath.Length - extension.Length);
					var assetAB = segments[1];
					var assetGUID = segments[2];
					var abContext = new AssetPath {
						Path = string.Intern(fullPath),
						ABName = string.Intern(assetAB),
					};
					if( segments.Length == 4 ) {
						AssetService.Get().Container.AddAssetBundleStrategy(assetAB, (BundleUnloadStrategy)( int.Parse(segments[3]) ));
					}

					if( m_asset2ABMap.TryGetValue(fullPath, out _) ) {
						Debug.LogError($"File name duplicate {fullPath}");
					}
					else {
						m_asset2ABMap.Add(fullPath, abContext);
					}
					m_guid2AssetPath.Add(assetGUID, fullPath);
					line = reader.ReadLine();
				}
			}
		}

		public override void ProvideAsync(AssetAsyncLoadHandle loadHandle, Type typ) {
			var operators = new List<AssetOperatorBase>();
			if (PrepareMainAbOperators(operators, loadHandle))
			{
				HandleAssetOperators(operators, loadHandle, typ);
			}
		}

		private bool TryGetABContext(string path, out AssetPath context) {
			if( !m_asset2ABMap.TryGetValue(path, out context) ) {
				Debug.LogError($"Can not file asset : {path}");
				return false;
			}

			return true;
		}

		public override AssetReference Provide(string path, AssetContainer container, Type typ) {
			var asset = ProvideAsset(path, container, typ);
			return new AssetReference(asset);
		}

		public override void ProvideSceneAsync(AssetAsyncLoadHandle loadHandle)
		{
			var operators = new List<AssetOperatorBase>();
			if (PrepareMainAbOperators(operators, loadHandle, true))
			{
				HandleAssetOperators(operators, loadHandle, null);
			}
		}

		public override void ProvideScene(string path, AssetContainer container)
		{
			if( !TryGetABContext(path, out var abPathContext) ) {
				return;
			}

			var mainAbInstance = HandleMainAbDependencies(abPathContext.ABName, container);

			if( !mainAbInstance.IsFinished ) {
				var ab = AssetBundle.LoadFromFile(DetermineLocation(mainAbInstance.ABPath));
				string[] scenePaths = ab.GetAllScenePaths();
				SceneManager.LoadScene(scenePaths[0]);
			}
		}

		public override bool Exist(string path) {
			return TryGetABContext(path, out _);
		}

		private AssetInstance ProvideAsset(string path, AssetContainer container, Type typ) {
			if( !TryGetABContext(path, out var abPathContext) ) {
				Debug.LogWarning($"Can not get ab context : {path}");
				return null;
			}

			if( !( container.TryGetAsset(path.GetHashCode()) is AssetInstance asset ) ) {
				asset = typ == typeof(GameObject) ? new PrefabAssetInstance(path) : new AssetInstance(path);
				container.Put(asset);
			}

			var mainAbInstance = HandleMainAbDependencies(abPathContext.ABName, container);

			if( !mainAbInstance.IsFinished ) {
				var ab = AssetBundle.LoadFromFile(DetermineLocation(mainAbInstance.ABPath));
				mainAbInstance.SetAssetBundle(ab, GetDirectDependencies(mainAbInstance.ABPath));
			}

			var unityObject = mainAbInstance.AB.LoadAsset(abPathContext.Path, typ);
			asset.SetAsset(unityObject, mainAbInstance);
			return asset;
		}
		/// <summary>
		/// 处理加载目标AB的全部依赖项
		/// </summary>
		/// <param name="abName">目标ab名</param>
		/// <param name="container">ab缓存</param>
		/// <returns></returns>
		private AssetBundleInstance HandleMainAbDependencies(string abName, AssetContainer container )
		{
			var mainAbHash = AssetBundleInstance.GenerateHash(abName);
			if( !( container.TryGetAsset(mainAbHash) is AssetBundleInstance mainAbInstance) ) {
				mainAbInstance = new AssetBundleInstance(abName);
				container.PutAB(mainAbInstance);
				var needLoadPaths = new List<AssetBundleInstance>();
				var allDependencies = m_manifest.GetAllDependencies(abName);
				foreach( var dependency in allDependencies ) {
					var depHash = AssetBundleInstance.GenerateHash(dependency);
					if( !( container.TryGetAsset(depHash) is AssetBundleInstance depAsset ) ) {
						depAsset = new AssetBundleInstance(dependency);
						container.PutAB(depAsset);
					}

					if( depAsset.IsFinished )
						continue;

					needLoadPaths.Add(depAsset);
				}

				foreach( var dependency in needLoadPaths ) {
					var ab = AssetBundle.LoadFromFile(DetermineLocation(dependency.ABPath));
					dependency.SetAssetBundle(ab, GetDirectDependencies(dependency.ABPath));
				}
			}

			return mainAbInstance;
		}

		private void HandleAssetOperators(List<AssetOperatorBase> operators, AssetAsyncLoadHandle loadHandle, Type t)
		{
			var op = new AssetOperators()
			{
				Operators = operators.ToArray()
			};
			op.Execute(loadHandle, t);
		}

		private AsyncABArrayOperator GenerateMainAbDependenciesOperator(string abName, out AssetRefObject mainAbInstance, AssetAsyncLoadHandle loadHandle)
		{
			var mainABHash = AssetBundleInstance.GenerateHash(abName);
			mainAbInstance = loadHandle.Container.TryGetAsset(mainABHash);
			if (mainAbInstance == null)
			{
				mainAbInstance = new AssetBundleInstance(abName);
				loadHandle.Container.PutAB((AssetBundleInstance) mainAbInstance);
				var allDependencies = m_manifest.GetAllDependencies(abName);
				if (allDependencies.Length > 0)
				{
					foreach (var dependency in allDependencies)
					{
						var depHash = AssetBundleInstance.GenerateHash(dependency);
						var depAsset = loadHandle.Container.TryGetAsset(depHash);
						if (depAsset == null)
						{
							loadHandle.Container.PutAB(new AssetBundleInstance(dependency));
						}
					}
				}

				return new AsyncABArrayOperator(allDependencies);
			}
			return null;
		}

		private bool PrepareMainAbOperators(List<AssetOperatorBase> operators, AssetAsyncLoadHandle loadHandle, bool isScene = false)
		{
			if( !AbContextChecker(loadHandle, out var abPathContext) ) {
				return false;
			}
			loadHandle.Location = abPathContext.Path;
			var mainAbName = abPathContext.ABName;
			var mainAbHash = AssetBundleInstance.GenerateHash(mainAbName);
			var mainAbInstance = loadHandle.Container.TryGetAsset(mainAbHash);
			
			var mainAbDependenciesOperator = GenerateMainAbDependenciesOperator(mainAbName, out mainAbInstance, loadHandle);
			if (mainAbDependenciesOperator != null)
			{
				operators.Add(mainAbDependenciesOperator);
			}
			if( !mainAbInstance.IsFinished ) {
				operators.Add(new AsyncABArrayOperator(new[] {abPathContext.ABName}));
			}

			if (isScene)
			{
				operators.Add(new AsyncABSceneOperator(mainAbHash));
			}
			else
			{
				operators.Add(new AsyncABAssetOperator(mainAbHash, loadHandle.Asset));
			}

			return true;
		}
		
		private bool AbContextChecker(AssetAsyncLoadHandle loadHandle, out AssetPath abPathContext)
		{
			if( !TryGetABContext(loadHandle.Location, out abPathContext ) ) {
				loadHandle.Asset.Status = AssetRefObject.AssetStatus.FAIL;
				loadHandle.Complete();
				return false;
			}

			return true;
		}
		
		internal override AssetInstance ProvideAssetWithGUID<T>(string guid, AssetContainer container, out string path) {
			if( m_guid2AssetPath.TryGetValue(guid, out path) ) {
				return ProvideAsset(path, container, typeof(T));
			}

			Debug.LogWarning($"Missing asset for guid {guid}");
			return null;
		}

		internal override string ConvertGUID2Path(string guid) {
			return m_guid2AssetPath.TryGetValue(guid, out var path) ? path : null;
		}

		public string[] GetDirectDependencies(string abName) {
			return m_manifest.GetDirectDependencies(abName);
		}

		public static string DetermineLocation(string path) {
			var streamingAsset = Path.Combine(PERSISTENT_DATA_PATH, path);
			return File.Exists(streamingAsset) ? streamingAsset : Path.Combine(STREAMING_ASSET_PATH, path);
		}
		
		public static string DetermineLocation(string path, out bool persistent) {
			var streamingAsset = Path.Combine(PERSISTENT_DATA_PATH, path);
			if( File.Exists(streamingAsset) ) {
				persistent = true;
				return streamingAsset;
			}

			persistent = false;
			return Path.Combine(STREAMING_ASSET_PATH, path);
		}
	}
}