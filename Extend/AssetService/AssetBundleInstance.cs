using System.Linq;
using Extend.Common;
using UnityEngine;

namespace Extend.AssetService {
	public class AssetBundleInstance : AssetRefObject {
		public AssetBundle AB { get; private set; }
		private string ABPath { get; }
		private AssetBundleInstance[] dependencies;

		public AssetBundleInstance(string abPath) {
			ABPath = string.Intern(abPath);
		}

		public void SetAssetBundle(AssetBundle ab, AssetBundleInstance[] deps) {
			AB = ab;
			dependencies = deps;
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