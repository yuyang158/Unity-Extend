using System;
using System.Collections;
using Extend.Common;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using XLua;

namespace Extend.Asset {
	[LuaCallCSharp, CSharpCallLua]
	public class AssetAsyncLoadHandle : IEnumerator {
		private AsyncOperationHandle m_handle;
		public AssetAsyncLoadHandle(AssetContainer container, AsyncOperationHandle handle) {
			Container = container;
			m_handle = handle;
			m_handle.Completed += Complete;
		}

		public AssetReference Result {
			get {
				if( !m_handle.IsDone ) {
					throw new Exception("Dont request result before load process is done.");
				}
				return new AssetReference(m_handle);
			}
		}

		public float Progress => m_handle.PercentComplete;
		
		public bool Cancel { get; set; }

		[BlackList]
		public AssetContainer Container { get; }

		[BlackList]
		public int AssetHashCode => m_handle.GetHashCode();

		public delegate void OnAssetLoadComplete(AssetAsyncLoadHandle handle);
		public event OnAssetLoadComplete OnComplete;

		[BlackList]
		internal AssetInstance Asset { get; private set; }

		[BlackList]
		private void Complete(AsyncOperationHandle handle) {
#if ASSET_LOG
			Debug.LogWarning($"Asset {handle.DebugName} load complete : {handle.Status}");
#endif
			Asset = Container.TryGetAsset(handle.GetHashCode()) as AssetInstance ?? 
			        (handle.Result is GameObject ? new PrefabAssetInstance(handle) : new AssetInstance(handle));
			if( Cancel ) {
				Asset.Release();
				return;
			}
			OnComplete?.Invoke(this);
		}

		[BlackList]
		public override string ToString() {
			return $"Load {m_handle.DebugName}";
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