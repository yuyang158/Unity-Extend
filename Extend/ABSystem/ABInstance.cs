using Common;
using ABSystem;
using UnityEngine;

namespace ABSystem {
	public class ABInstance : RefObject {
		public AssetBundle AB { get; }
		private string ABPath { get; }
		private readonly ABInstance[] dependencies;

		public ABInstance(AssetBundle ab, string abPath, ABInstance[] dependencies) {
			AB = ab;
			ABPath = abPath;
			this.dependencies = dependencies;
			foreach( var dependency in dependencies ) {
				dependency.IncRef();
			}
		}

		public override void Destroy() {
			foreach( var dependency in dependencies ) {
				dependency.Release();
			}

			var service =(ABService)CSharpServiceManager.Get( CSharpServiceManager.ServiceType.AB_SERVICE );
			service.RemoveAB( ABPath );
			
			AB.Unload( false );
		}
	}
}