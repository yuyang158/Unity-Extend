using System;
using System.Collections;
using Extend.AssetService.AssetProvider;

namespace Extend.AssetService {
	public class AssetAsyncLoadHandle : IEnumerator {
		public AssetAsyncLoadHandle(AssetContainer container, AssetLoadProvider provider, string path) {
			Container = container;
			Provider = provider;
			Location = string.Intern(Provider.FormatAssetPath(path));
		}

		public AssetReference Result { get; private set; }
		public float Progress { get; private set; }
		public string Location { get; set; }
		public AssetContainer Container { get; }
		public AssetLoadProvider Provider { get; }

		public int AssetHashCode => AssetInstance.GenerateHash(Location);
		public event Action<AssetAsyncLoadHandle, bool> OnComplete;

		public void Complete(AssetInstance asset) {
			if( asset == null ) {
				OnComplete?.Invoke(this, false);
			}
			else {
				Result = new AssetReference(asset);
				OnComplete?.Invoke(this, asset.Status == AssetRefObject.AssetStatus.DONE);
			}
		}

		public void Execute() {
			var hashCode = AssetInstance.GenerateHash(Location);
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

			Provider.ProvideAsync(this);
		}

		private void OnAssetReady(AssetRefObject asset) {
			if( asset.IsFinished ) {
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