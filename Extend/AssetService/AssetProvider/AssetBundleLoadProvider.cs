using System;
using System.Collections.Generic;
using System.IO;
using Extend.AssetService.AssetOperator;
using UnityEngine;
using UnityEngine.Assertions;

namespace Extend.AssetService.AssetProvider {
	public class AssetBundleLoadProvider : AssetLoadProvider {
		private struct AssetBundlePath {
			public string Path;
			public string ABName;
		}

		private AssetBundleManifest manifest;
		private static string streamingAssetsPath;
		private static string persistentDataPath;
		private readonly Dictionary<string, AssetBundlePath> asset2ABMap = new Dictionary<string, AssetBundlePath>();
		private readonly Dictionary<string, string> guid2AssetPath = new Dictionary<string, string>();

		public override string FormatAssetPath(string path) {
			if( path.StartsWith("assets") ) {
				return path;
			}
			
			return "assets/resources/" + base.FormatAssetPath(path).ToLower();
		}

		public override void Initialize() {
#if UNITY_IPHONE
			const var platform = "iOS";
#elif UNITY_ANDROID
			const string platform = "Android";
#else
			const var platform = "StandaloneWindows";
#endif
			streamingAssetsPath = Path.Combine(Application.streamingAssetsPath, "ABBuild", platform);
			persistentDataPath = Path.Combine(Application.persistentDataPath, "ABBuild", platform);

			var manifestPath = DetermineLocation(platform);
			var manifestAB = AssetBundle.LoadFromFile(manifestPath);
			manifest = manifestAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

			var mapPath = DetermineLocation("package.conf");
			using( var reader = new StreamReader(mapPath) ) {
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
					asset2ABMap.Add(fullPath, abContext);
					guid2AssetPath.Add(assetGUID, fullPath);
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
				var allDependencies = manifest.GetAllDependencies(abPathContext.ABName);
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
			if( !asset2ABMap.TryGetValue(path, out context) ) {
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
				asset = new AssetInstance(path);
				container.Put(asset);
			}

			var mainABHash = AssetBundleInstance.GenerateHash(abPathContext.ABName);
			if( !( container.TryGetAsset(mainABHash) is AssetBundleInstance mainABInstance ) ) {
				mainABInstance = new AssetBundleInstance(abPathContext.ABName);
				container.Put(mainABInstance);
				var needLoadPaths = new List<AssetBundleInstance>();
				var allDependencies = manifest.GetAllDependencies(abPathContext.ABName);
				foreach( var dependency in allDependencies ) {
					var depHash = AssetBundleInstance.GenerateHash(dependency);
					var depAsset = container.TryGetAsset(depHash) as AssetBundleInstance;
					if( depAsset == null ) {
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

		internal override AssetInstance ProvideAssetWithGUID<T>(string guid, AssetContainer container) {
			return guid2AssetPath.TryGetValue(guid, out var path) ? ProvideAsset(path, container, typeof(T)) : null;
		}

		internal override string ConvertGUID2Path(string guid) {
			return guid2AssetPath.TryGetValue(guid, out var path) ? path : null;
		}

		public string[] GetDirectDependencies(string abName) {
			return manifest.GetDirectDependencies(abName);
		}

		public static string DetermineLocation(string path) {
			var streamingAsset = Path.Combine(streamingAssetsPath, path);
			if( File.Exists(streamingAsset) ) {
				return streamingAsset;
			}

			return persistentDataPath + path;
		}
	}
}