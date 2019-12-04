using System.Collections.Generic;
using Extend.Common;
using UnityEngine;

namespace Extend.AssetService {
	public class ResourcesService : IAssetService {
		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.ASSET_SERVICE;
		private readonly Dictionary<string, AssetInstance> loadedAsset = new Dictionary<string, AssetInstance>();
		public void Initialize() {
		}

		public void Destroy() {
			
		}

		public void Update() {
		}

		public AssetReference Load(string path) {
			if( loadedAsset.TryGetValue( path, out var asset ) ) {
				return new AssetReference(asset);
			}

			var unityObject = Resources.Load(path);
			if( !unityObject ) {
				return null;
			}
			asset = new AssetInstance(unityObject, path, null);
			loadedAsset.Add(path, asset);
			return new AssetReference(asset);
		}
		
		public void RemoveAsset(string assetPath) {
			loadedAsset.Remove( assetPath );
		}
	}
}