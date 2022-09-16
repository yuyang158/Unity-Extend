using Extend.Common;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace Extend.Asset {
	internal class AssetInstance : AssetRefObject {
		public Object UnityObject { get; private set; }
		protected AsyncOperationHandle m_handle { get; }
		public float CreateTime;

		public AssetInstance(AsyncOperationHandle handle) {
			m_handle = handle;
			AssetService.Get().Container.Put(this);
			#if ASSET_LOG
			Debug.LogWarning("Put asset : " + handle.DebugName);
			#endif
			m_debugNameCache = handle.DebugName;

			if( m_handle.IsDone ) {
				SetAsset(m_handle.Result as Object);
			}
			else {
				m_handle.Completed += _ => {
					SetAsset(m_handle.Result as Object);
				};
			}
			Addressables.ResourceManager.Acquire(m_handle);
		}

		protected virtual void SetAsset(Object unityObj) {
			UnityObject = unityObj;
			Status = UnityObject ? AssetStatus.DONE : AssetStatus.FAIL;
			if( Status == AssetStatus.DONE ) {
				StatService.Get().Increase(StatService.StatName.ASSET_COUNT, 1);
				CreateTime = Time.realtimeSinceStartup;
			}
		}

		internal virtual void Update() {
			
		}

		public override void Destroy() {
			if( Status == AssetStatus.DONE ) {
				StatService.Get().Increase(StatService.StatName.ASSET_COUNT, -1);
			}
			Status = AssetStatus.DESTROYED;
			UnityObject = null;
			Addressables.ResourceManager.Release(m_handle);
		}
		
		public static int GenerateHash(string path) {
			return path.GetHashCode();
		}

		public override int GetHashCode() {
			return m_handle.GetHashCode();
		}
	}
}