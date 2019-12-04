using Extend.Common;
using UnityEngine;

namespace Extend.AssetService {
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

			var service = CSharpServiceManager.Get<ABService>( CSharpServiceManager.ServiceType.ASSET_SERVICE );
			service.RemoveAB( ABPath );
			
			AB.Unload( false );
		}
	}
}