using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

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

		private static AssetBundleManifest m_manifest;
		public static AssetBundleManifest Manifest => m_manifest;
		private static string STREAMING_ASSET_PATH;
		private static string PERSISTENT_DATA_PATH;
		private readonly Dictionary<string, AssetPath> m_asset2ABMap = new Dictionary<string, AssetPath>();
		private readonly Dictionary<string, string> m_guid2AssetPath = new Dictionary<string, string>();

		public override string FormatAssetPath(string path) {
			path = base.FormatAssetPath(path).ToLower();
			if( path.StartsWith("assets") ) {
				return path;
			}

			return "assets/resources/" + path;
		}

		public override string FormatScenePath(string path) {
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
			manifestAB.Unload(false);

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

		private static AssetBundleInstance FindOrCreateABInstance(string path, AssetContainer container) {
			var hash = AssetBundleInstance.GenerateHash(path);
			var abInstance = container.TryGetAsset(hash) as AssetBundleInstance ?? new AssetBundleInstance(path);
			return abInstance;
		}

		public override void ProvideAsync(AssetAsyncLoadHandle loadHandle, Type typ) {
			if( !TryGetABContext(loadHandle.Location, out var abPathContext) ) {
				return;
			}
			var abInstance = FindOrCreateABInstance(abPathContext.ABName, loadHandle.Container);
			abInstance.LoadAssetAsync(abPathContext.Path, unityObject => {
				loadHandle.Asset.SetAsset(unityObject, abInstance);
			}, typ);
		}

		private bool TryGetABContext(string path, out AssetPath context) {
			return m_asset2ABMap.TryGetValue(path, out context);
		}

		public override AssetReference Provide(string path, AssetContainer container, Type typ) {
			path = FormatAssetPath(path);
			var asset = ProvideAsset(path, container, typ);
			return new AssetReference(asset);
		}

		public override void ProvideSceneAsync(AssetAsyncLoadHandle loadHandle, bool add) {
			
		}

		public override void ProvideScene(string path, AssetContainer container, bool add) {
			if( !TryGetABContext(path, out var abPathContext) ) {
				Debug.LogError($"Load scene failed, not found asset bundle config for : {path}");
				return;
			}

			var abInstance = FindOrCreateABInstance(abPathContext.ABName, container);
			abInstance.Load();
			SceneManager.LoadScene(abInstance.GetScenePath(),
				add ? LoadSceneMode.Additive : LoadSceneMode.Single);
			//abInstance.Destroy();
		}

		public override bool Exist(string path) {
			return TryGetABContext(path, out _);
		}

		private AssetInstance ProvideAsset(string path, AssetContainer container, Type typ) {
			var hash = AssetInstance.GenerateHash(path);
			if( container.TryGetAsset(hash) is AssetInstance asset ) {
				return asset;
			}

			if( !TryGetABContext(path, out var abPathContext) ) {
				Debug.LogWarning($"Can not get ab context : {path}");
				return null;
			}

			asset = typ == typeof(GameObject) ? new PrefabAssetInstance(path) : new AssetInstance(path);
			var abInstance = FindOrCreateABInstance(abPathContext.ABName, container);
			asset.SetAsset(abInstance.LoadAsset(abPathContext.Path, typ), abInstance);
			return asset;
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