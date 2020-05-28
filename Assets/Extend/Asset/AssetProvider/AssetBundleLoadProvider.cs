using System;
using System.Collections.Generic;
using System.IO;
using Extend.Asset.AssetOperator;
using UnityEngine;
using UnityEngine.Assertions;

#if UNITY_ANDROID
using System.Threading;
using UnityEngine.Networking;
#endif

namespace Extend.Asset.AssetProvider {
	public class AssetBundleLoadProvider : AssetLoadProvider {
		private struct AssetBundlePath {
			public string Path;
			public string ABName;
		}

		private AssetBundleManifest m_manifest;
		private static string STREAMING_ASSET_PATH;
		private static string PERSISTENT_DATA_PATH;
		private readonly Dictionary<string, AssetBundlePath> m_asset2ABMap = new Dictionary<string, AssetBundlePath>();
		private readonly Dictionary<string, string> m_guid2AssetPath = new Dictionary<string, string>();

		public override string FormatAssetPath(string path) {
			if( path.StartsWith("assets") ) {
				return path;
			}

			return "assets/resources/" + base.FormatAssetPath(path).ToLower();
		}

		public override void Initialize() {
#if UNITY_IOS
			const string platform = "iOS";
#elif UNITY_ANDROID
			const string platform = "Android";
#else
			const string platform = "StandaloneWindows";
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
					Assert.AreEqual(segments.Length, 3);
					var fullPath = segments[0];
					var extension = Path.GetExtension(fullPath);
					fullPath = fullPath.Substring(0, fullPath.Length - extension.Length);
					var assetPath = segments[0];
					var assetAB = segments[1];
					var assetGUID = segments[2];
					var abContext = new AssetBundlePath {
						Path = string.Intern(assetPath),
						ABName = string.Intern(assetAB)
					};
					m_asset2ABMap.Add(fullPath, abContext);
					m_guid2AssetPath.Add(assetGUID, fullPath);
					line = reader.ReadLine();
				}
			}
		}

		public override void ProvideAsync(AssetAsyncLoadHandle loadHandle, Type typ) {
			if( !TryGetABContext(loadHandle.Location, out var abPathContext) ) {
				loadHandle.Asset.Status = AssetRefObject.AssetStatus.FAIL;
				loadHandle.Complete();
				return;
			}

			var operators = new List<AssetOperatorBase>(4);
			var mainABHash = AssetBundleInstance.GenerateHash(abPathContext.ABName);
			var mainABInstance = loadHandle.Container.TryGetAsset(mainABHash);
			if( mainABInstance == null ) {
				mainABInstance = new AssetBundleInstance(abPathContext.ABName);
				loadHandle.Container.Put(mainABInstance);
				var allDependencies = m_manifest.GetAllDependencies(abPathContext.ABName);
				if( allDependencies.Length > 0 ) {
					foreach( var dependency in allDependencies ) {
						var depHash = AssetBundleInstance.GenerateHash(dependency);
						var depAsset = loadHandle.Container.TryGetAsset(depHash);
						if( depAsset == null ) {
							loadHandle.Container.Put(new AssetBundleInstance(dependency));
						}
					}

					operators.Add(new AsyncABArrayOperator(allDependencies));
				}
			}

			if( !mainABInstance.IsFinished ) {
				operators.Add(new AsyncABArrayOperator(new[] {abPathContext.ABName}));
			}

			operators.Add(new AsyncABAssetOperator(mainABHash, loadHandle.Asset));
			var op = new AssetOperators {
				Operators = operators.ToArray()
			};
			loadHandle.Location = abPathContext.Path;
			op.Execute(loadHandle, typ);
		}

		private bool TryGetABContext(string path, out AssetBundlePath context) {
			if( !m_asset2ABMap.TryGetValue(path, out context) ) {
				Debug.LogError($"Can not file asset at {path}");
				return false;
			}

			return true;
		}

		public override AssetReference Provide(string path, AssetContainer container, Type typ) {
			var asset = ProvideAsset(path, container, typ);
			return new AssetReference(asset);
		}

		private AssetInstance ProvideAsset(string path, AssetContainer container, Type typ) {
			if( !TryGetABContext(path, out var abPathContext) ) {
				return null;
			}

			if( !( container.TryGetAsset(AssetBundleInstance.GenerateHash(path)) is AssetInstance asset ) ) {
				asset = typ == typeof(GameObject) ? new PrefabAssetInstance(path) : new AssetInstance(path);
				container.Put(asset);
			}

			var mainABHash = AssetBundleInstance.GenerateHash(abPathContext.ABName);
			if( !( container.TryGetAsset(mainABHash) is AssetBundleInstance mainABInstance ) ) {
				mainABInstance = new AssetBundleInstance(abPathContext.ABName);
				container.Put(mainABInstance);
				var needLoadPaths = new List<AssetBundleInstance>();
				var allDependencies = m_manifest.GetAllDependencies(abPathContext.ABName);
				foreach( var dependency in allDependencies ) {
					var depHash = AssetBundleInstance.GenerateHash(dependency);
					if( !( container.TryGetAsset(depHash) is AssetBundleInstance depAsset ) ) {
						depAsset = new AssetBundleInstance(dependency);
						container.Put(depAsset);
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

			if( !mainABInstance.IsFinished ) {
				var ab = AssetBundle.LoadFromFile(DetermineLocation(mainABInstance.ABPath));
				mainABInstance.SetAssetBundle(ab, GetDirectDependencies(mainABInstance.ABPath));
			}

			var unityObject = mainABInstance.AB.LoadAsset(abPathContext.Path, typ);
			asset.SetAsset(unityObject, mainABInstance);
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