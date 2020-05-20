using System;
using System.Collections;
using Extend.Asset.AssetProvider;
using Extend.Common;
using Extend.Common.Lua;
using UnityEngine;

namespace Extend.Asset {
	[LuaCallCSharp, CSharpCallLua]
	public class AssetAsyncLoadHandle : IEnumerator {
		public AssetAsyncLoadHandle(AssetContainer container, AssetLoadProvider provider, string path) {
			Container = container;
			Provider = provider;
			Location = string.Intern(Provider.FormatAssetPath(path));
		}

		public AssetReference Result => new AssetReference(Asset);

		public float Progress { get; private set; }
		public string Location { get; set; }

		[BlackList]
		public AssetContainer Container { get; }

		[BlackList]
		public AssetLoadProvider Provider { get; }

		[BlackList]
		public int AssetHashCode => AssetInstance.GenerateHash(Location);

		public delegate void OnAssetLoadComplete(AssetAsyncLoadHandle handle);
		public event OnAssetLoadComplete OnComplete;
		public AssetInstance Asset { get; private set; }

		[BlackList]
		public void Complete() {
			Progress = 1;
			OnComplete?.Invoke(this);
		}

		private static readonly WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();

		private IEnumerator AsyncLoadedAssetCallback() {
			yield return frameEnd;
			Complete();
		}

		[BlackList]
		public void Execute(Type typ) {
			var hashCode = AssetInstance.GenerateHash(Location);
			Asset = Container.TryGetAsset(hashCode) as AssetInstance;
			if( Asset == null ) {
				Asset = new AssetInstance(Location);
				Container.Put(Asset);
			}
			else if( Asset.IsFinished ) {
				var service = CSharpServiceManager.Get<GlobalCoroutineRunnerService>(CSharpServiceManager.ServiceType.COROUTINE_SERVICE);
				service.StartCoroutine(AsyncLoadedAssetCallback());
				return;
			}

			Asset.OnStatusChanged += OnAssetReady;
			Provider.ProvideAsync(this, typ);
		}

		private void OnAssetReady(AssetRefObject asset) {
			if( asset.IsFinished ) {
				asset.OnStatusChanged -= OnAssetReady;
				Complete();
			}
		}

		[BlackList]
		public override string ToString() {
			return $"Load {Location}";
		}

		[BlackList]
		public bool MoveNext() {
			return !Asset.IsFinished;
		}

		[BlackList]
		public void Reset() {
		}

		[BlackList]
		public object Current => Result;
	}
}