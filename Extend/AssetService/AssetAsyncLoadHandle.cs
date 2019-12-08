using System;
using System.Collections;

namespace Extend.AssetService {
	public class AssetAsyncLoadHandle : IEnumerator {
		public AssetAsyncLoadHandle(AssetContainer container, AssetAsyncProvider provider, string path) {
			Container = container;
			Provider = provider;
			Location = string.Intern(path);
		}

		public AssetReference Result { get; private set; }
		public float Progress { get; private set; }
		public string Location { get; set; }
		public AssetContainer Container { get; }
		public AssetAsyncProvider Provider { get; }

		public int AssetHashCode => AssetInstance.GenerateHash(Location);
		public event Action<AssetAsyncLoadHandle, bool> OnComplete;

		public void Complete(AssetInstance asset) {
			if( asset == null ) {
				OnComplete(this, false);
			}
			else {
				Result = new AssetReference(asset);
				OnComplete(this, asset.Status == AssetRefObject.AssetStatus.DONE);
			}
		}

		public void Execute() {
			var hashCode = Provider.GetAssetHashCode(Location);
			var asset = Container.TryGetAsset(hashCode);
			if( asset != null ) {
				if( asset.Status == AssetRefObject.AssetStatus.DONE ) {
					Progress = 1;
					Result = new AssetReference(asset as AssetInstance);
					OnComplete?.Invoke(this, true);
					return;
				}

				if( asset.Status == AssetRefObject.AssetStatus.FAIL ) {
					OnComplete?.Invoke(this, false);
					return;
				}
			}
			else {
				asset = new AssetInstance(Location);
				asset.OnStatusChanged += OnAssetReady;
				Container.Put(asset);
			}

			Container.Put(asset);
			Provider.Provide(this);
		}

		private void OnAssetReady(AssetRefObject.AssetStatus status, AssetRefObject asset) {
			if( status == AssetRefObject.AssetStatus.DONE ) {
				asset.OnStatusChanged -= OnAssetReady;
				Result = new AssetReference(asset as AssetInstance);
				OnComplete?.Invoke(this, true);
			}
		}

		public override string ToString() {
			return $"Load {Location}";
		}

		public bool MoveNext() {
			return Result == null;
		}

		public void Reset() {
		}

		public object Current => Result;
	}
}