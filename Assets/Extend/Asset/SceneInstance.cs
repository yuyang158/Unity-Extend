using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using XLua;
using AddressableSceneInstance = UnityEngine.ResourceManagement.ResourceProviders.SceneInstance;

namespace Extend.Asset {
	[LuaCallCSharp]
	public class SceneInstance {
		private AddressableSceneInstance m_scene;
		private bool m_disposed;
		private readonly AsyncOperationHandle m_handle;
		
		[BlackList]
		public SceneInstance(AsyncOperationHandle handle) {
			m_handle = handle;
			if( handle.IsDone ) {
				m_scene = (AddressableSceneInstance)handle.Result;
			}
			else {
				m_handle.Completed += operationHandle => {
					m_scene = (AddressableSceneInstance)operationHandle.Result;
				};
			}
		}

		public Scene GetScene() {
			return m_scene.Scene;
		}

		private static void PerformDestroy() {
			// Addressables.Release(m_handle);
			AssetService.Get().FullCollect();
		}

		public void Destroy() {
			if( m_disposed ) {
				return;
			}

			m_disposed = true;
			PerformDestroy();
		}
	}
}