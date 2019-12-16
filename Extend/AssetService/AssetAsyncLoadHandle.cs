using System;
using System.Collections;
using Extend.AssetService.AssetProvider;
using XLua;

namespace Extend.AssetService {
	[LuaCallCSharp]
	public class AssetAsyncLoadHandle : IEnumerator {
		public AssetAsyncLoadHandle(AssetContainer container, AssetLoadProvider provider, string path) {
			Container = container;
			Provider = provider;
			Location = string.Intern(Provider.FormatAssetPath(path));
		}

		public AssetReference Result { get; private set; }
		public float Progress { get; private set; }
		public string Location { get; set; }
		[BlackList]
		public AssetContainer Container { get; }
		[BlackList]
		public AssetLoadProvider Provider { get; }

		public int AssetHashCode => AssetInstance.GenerateHash(Location);
		public event Action<AssetAsyncLoadHandle, bool> OnComplete;

		[BlackList]
		public void Complete(AssetInstance asset) {
			if( asset == null ) {
				OnComplete?.Invoke(this, false);
			}
			else {
				Result = new AssetReference(asset);
				OnComplete?.Invoke(this, asset.Status == AssetRefObject.AssetStatus.DONE);
			}
		}

		[BlackList]
		public void Execute() {
			var hashCode = AssetInstance.GenerateHash(Location);
			var asset = Container.TryGetAsset(hashCode);
			if( asset != null ) {
				if( asset.IsFinished ) {
					Progress = 1;
					Result = new AssetReference(asset as AssetInstance);
					OnComplete?.Invoke(this, asset.Status == AssetRefObject.AssetStatus.DONE);
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

		[BlackList]
		public override string ToString() {
			return $"Load {Location}";
		}

		[BlackList]
		public bool MoveNext() {
			return Result == null;
		}

		[BlackList]
		public void Reset() {
		}

		[BlackList]
		public object Current => Result;
	}
}