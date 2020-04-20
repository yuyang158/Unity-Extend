using System;
using System.Linq;
using Extend.Common;
using UnityEngine;

namespace Extend.Asset {
	public class AssetBundleInstance : AssetRefObject {
		public AssetBundle AB { get; private set; }
		public string ABPath { get; }
		private AssetBundleInstance[] dependencies;

		public AssetBundleInstance(string abPath) {
			ABPath = string.Intern(abPath);
		}

		public void SetAssetBundle(AssetBundle ab, string[] deps) {
			AB = ab;
			var service = CSharpServiceManager.Get<AssetService>(CSharpServiceManager.ServiceType.ASSET_SERVICE);
			var assetBundles = new AssetBundleInstance[deps.Length];
			for( var i = 0; i < deps.Length; i++ ) {
				var hash = GenerateHash(deps[i]);
				var dep = service.Container.TryGetAsset(hash);
				assetBundles[i] = dep as AssetBundleInstance ?? throw new Exception($"Can not find dependency : {deps[i]}");
			}
			
			dependencies = assetBundles;
			foreach( var dependency in dependencies ) {
				dependency.IncRef();
			}

			if( AB ) {
				if( dependencies.Any(dependency => dependency.Status != AssetStatus.DONE) ) {
					Status = AssetStatus.FAIL;
					return;
				}

				Status = AssetStatus.DONE;
			}
			else {
				Status = AssetStatus.FAIL;
			}
		}

		public override void Destroy() {
			foreach( var dependency in dependencies ) {
				dependency.Release();
			}
			AB.Unload( false );
		}

		public static int GenerateHash(string path) {
			var hash = path + ".ab";
			return hash.GetHashCode();
		}

		public override int GetHashCode() {
			return GenerateHash(ABPath);
		}

		public override string ToString() {
			return ABPath;
		}
	}
}