using System;
using Extend.Asset;
using Extend.Common;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using XLua;

namespace Extend.SceneManagement {
	[LuaCallCSharp]
	public class SceneLoadManager : IService {
		public static SceneLoadManager Get() {
			return CSharpServiceManager.Get<SceneLoadManager>(CSharpServiceManager.ServiceType.SCENE_LOAD);
		}

		public int ServiceType => (int)CSharpServiceManager.ServiceType.SCENE_LOAD;
		public void Initialize() {
			SceneManager.sceneUnloaded += OnSceneUnloaded;
		}

		public SceneInstance LoadScene(string scenePath, bool additive) {
			Debug.LogWarning($"Start load scene {scenePath}.");
			var scene = AssetService.Get().LoadScene(scenePath, additive);
			return scene;
		}

		public void LoadSceneAsync(string scenePath, bool additive, Action<SceneInstance> callback = null) {
			Debug.LogWarning($"Start async load scene {scenePath}.");
			AssetService.Get().LoadSceneAsync(scenePath, additive, instance => {
				callback?.Invoke(instance);
				var sceneShadowSection = $"SCENE.{instance.GetScene().name}.SHADOW.CASCADE";
				if( GameSystemSetting.Get().SystemSetting.SectionExist(sceneShadowSection) ) {
					var cascadeCount = GameSystemSetting.Get().SystemSetting.GetInt(sceneShadowSection, "CascadeCount");
					var shadowDistance = GameSystemSetting.Get().SystemSetting.GetInt(sceneShadowSection, "ShadowDistance");
					var renderPipelineAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
					renderPipelineAsset.shadowCascadeCount = cascadeCount;
					renderPipelineAsset.shadowDistance = shadowDistance;
				}
			});
		}

		public void UnloadScene(string scenePath) {
			Debug.LogWarning($"Start unload scene {scenePath}.");
		}

		private static void OnSceneUnloaded(Scene scene) {
			AssetService.Get().FullCollect();
		}

		public void Destroy() {
			SceneManager.sceneUnloaded -= OnSceneUnloaded;
		}
	}
}