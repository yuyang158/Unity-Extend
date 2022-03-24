using System;
using System.Collections.Generic;
using Extend.Asset;
using Extend.Common;
using UnityEngine;
using UnityEngine.SceneManagement;
using XLua;

namespace Extend.SceneManagement {
	[LuaCallCSharp]
	public class SceneLoadManager : IService {
		public static SceneLoadManager Get() {
			return CSharpServiceManager.Get<SceneLoadManager>(CSharpServiceManager.ServiceType.SCENE_LOAD);
		}

		private readonly Dictionary<string, SceneInstance> m_loadedScenes = new();
		public int ServiceType => (int)CSharpServiceManager.ServiceType.SCENE_LOAD;
		public void Initialize() {
			SceneManager.sceneUnloaded += OnSceneUnloaded;
		}

		public SceneInstance LoadScene(string scenePath, bool additive) {
			var scene = AssetService.Get().LoadScene(scenePath, additive);
			m_loadedScenes.Add(scenePath, scene);
			return scene;
		}

		public void LoadSceneAsync(string scenePath, bool additive, Action<SceneInstance> callback = null) {
			AssetService.Get().LoadSceneAsync(scenePath, additive, instance => {
				m_loadedScenes.Add(scenePath, instance);
				callback?.Invoke(instance);
			});
		}

		public void UnloadScene(string scenePath) {
			Debug.LogWarning($"Start unload scene {scenePath}.");
			if( !m_loadedScenes.TryGetValue(scenePath, out var sceneInstance) ) {
				return;
			}

			SceneManager.UnloadSceneAsync(sceneInstance.GetScene());
		}

		private void OnSceneUnloaded(Scene scene) {
			if( !m_loadedScenes.TryGetValue(scene.path, out var instance) ) {
				return;
			}
			instance.Destroy();
		}

		public void Destroy() {
			SceneManager.sceneUnloaded -= OnSceneUnloaded;
		}
	}
}