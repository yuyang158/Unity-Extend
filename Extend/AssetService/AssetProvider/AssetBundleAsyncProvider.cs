using System.Collections.Generic;
using System.IO;
using Extend.AssetService.AssetOperator;
using UnityEngine;
using UnityEngine.Assertions;

namespace Extend.AssetService.AssetProvider {
	public class AssetBundleAsyncProvider : AssetAsyncProvider {
		private struct AssetBundlePath {
			public string Path;
			public string ABName;
		}

		private AssetBundleManifest manifest;
		private static string streamingAssetsPath;
		private static string persistentDataPath;
		private readonly Dictionary<string, AssetBundlePath> asset2ABMap = new Dictionary<string, AssetBundlePath>();

		public override void Initialize() {
			string platform;
			if( Application.platform == RuntimePlatform.IPhonePlayer ) {
				platform = "iOS";
			}
			else if( Application.platform == RuntimePlatform.Android ) {
				platform = "Android";
			}
			else {
				platform = "StandaloneWindows64";
			}
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
					Assert.AreEqual(segments.Length, 2);
					var fullPath = segments[0];
					var extension = Path.GetExtension(fullPath);
					fullPath = fullPath.Substring(17, fullPath.Length - 17 - extension.Length);
					asset2ABMap.Add(fullPath, new AssetBundlePath() {
						Path = segments[0],
						ABName = segments[1]
					});
					line = reader.ReadLine();
				}
			}
		}

		public override void Provide(AssetAsyncLoadHandle loadHandle) {
			if( !asset2ABMap.TryGetValue(loadHandle.Location.ToLower(), out var abPathContext) ) {
				Debug.LogError($"Can not file asset at {loadHandle.Location}");
				loadHandle.Complete(null);
			}

			var operators = new List<AssetOperatorBase>(4);
			var asset = loadHandle.Container.TryGetAsset(loadHandle.AssetHashCode);
			if( asset == null ) {
				asset = new AssetInstance(loadHandle.Location);
				loadHandle.Container.Put(asset);
			}

			var mainABHash = AssetBundleInstance.GenerateHash(abPathContext.ABName);
			var mainABInstance = loadHandle.Container.TryGetAsset(mainABHash);
			if( mainABInstance == null ) {
				mainABInstance = new AssetBundleInstance(abPathContext.ABName);
				loadHandle.Container.Put(mainABInstance);
				var allDependencies = manifest.GetAllDependencies(abPathContext.ABName);
				foreach( var dependency in allDependencies ) {
					var depHash = AssetBundleInstance.GenerateHash(dependency);
					var depAsset = loadHandle.Container.TryGetAsset(depHash);
					if( depAsset == null ) {
						loadHandle.Container.Put(new AssetBundleInstance(dependency));
					}
				}
				operators.Add(new ABAsyncGroupOperator(allDependencies));
			}

			if( mainABInstance.Status != AssetRefObject.AssetStatus.DONE ) {
				operators.Add(new ABAsyncGroupOperator(new[] {abPathContext.ABName}));
			}
			operators.Add(new ABAssetAsyncOperation(mainABHash, asset as AssetInstance));
			var op = new AssetOperators {
				Operators = operators.ToArray()
			};
			op.Execute(loadHandle);
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