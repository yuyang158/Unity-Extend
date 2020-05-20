using System;
using System.Linq;
using Extend.Common;
using UnityEngine;

namespace Extend.Asset {
	public class AssetBundleInstance : AssetRefObject {
		public AssetBundle AB { get; private set; }
		public string ABPath { get; }
		private AssetBundleInstance[] m_dependencies;

		public AssetBundleInstance(string abPath) {
			ABPath = string.Intern(abPath);
		}

		public void SetAssetBundle(AssetBundle ab, string[] dependencies) {
			AB = ab;
			var service = CSharpServiceManager.Get<AssetService>(CSharpServiceManager.ServiceType.ASSET_SERVICE);
			var assetBundles = new AssetBundleInstance[dependencies.Length];
			for( var i = 0; i < dependencies.Length; i++ ) {
				var hash = GenerateHash(dependencies[i]);
				var dep = service.Container.TryGetAsset(hash);
				assetBundles[i] = dep as AssetBundleInstance ?? throw new Exception($"Can not find dependency : {dependencies[i]}");
			}
			
			m_dependencies = assetBundles;
			foreach( var dependency in this.m_dependencies ) {
				dependency.IncRef();
			}

			if( AB ) {
				if( m_dependencies.Any(dependency => dependency.Status != AssetStatus.DONE) ) {
					Status = AssetStatus.FAIL;
					return;
				}

				Status = AssetStatus.DONE;
				var statService = CSharpServiceManager.Get<StatService>(CSharpServiceManager.ServiceType.STAT);
				statService.Increase(StatService.StatName.ASSET_BUNDLE_COUNT, 1);
			}
			else {
				Status = AssetStatus.FAIL;
			}
		}

		public override void Destroy() {
			if(Status != AssetStatus.DONE)
				return;
			foreach( var dependency in m_dependencies ) {
				dependency.Release();
			}
			AB.Unload( false );
			UnityEngine.Object.Destroy(AB);
			var statService = CSharpServiceManager.Get<StatService>(CSharpServiceManager.ServiceType.STAT);
			statService.Increase(StatService.StatName.ASSET_BUNDLE_COUNT, -1);
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