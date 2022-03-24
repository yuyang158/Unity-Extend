#if !UNITY_EDITOR
using System.Text;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
#endif
using DG.Tweening;
using Extend.Asset;
using Extend.Common;
using Extend.DebugUtil;
using Extend.LuaUtil;
using Extend.Network;
using Extend.UI.i18n;
using Extend.Render;
using Extend.SceneManagement;
using UnityEngine;

namespace Extend {
	internal static class StaticServiceInitializer {
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		private static void OnInitAssembliesLoaded() {
			CSharpServiceManager.Initialize();
			CSharpServiceManager.Register(new ErrorLogToFile());
			CSharpServiceManager.Register(new StatService());
			CSharpServiceManager.Register(new AssetService());
#if !UNITY_EDITOR
			var urpAsset = AssetService.Get().Load<UniversalRenderPipelineAsset>("Assets/Settings/UniversalRP-HighQuality.asset");
			GraphicsSettings.renderPipelineAsset = urpAsset.GetObject() as UniversalRenderPipelineAsset;
			QualitySettings.renderPipeline = urpAsset.GetObject() as UniversalRenderPipelineAsset;
#endif
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void OnInitBeforeSceneLoad() {
			Application.runInBackground = true;
			DOTween.Init(false, true, LogBehaviour.Default);

			CSharpServiceManager.Register(new GameSystem());
			CSharpServiceManager.Register(new RenderFeatureService());
			CSharpServiceManager.Register(new SpriteAssetService());
			CSharpServiceManager.Register(new LuaVM());
			CSharpServiceManager.Register(new TickService());
			CSharpServiceManager.Register(new I18nService());
			CSharpServiceManager.Register(new SceneLoadManager());


#if !UNITY_EDITOR
			var builder = new StringBuilder(2048);
			builder.AppendLine($"Unity: {Application.unityVersion}");
			builder.AppendLine($"App : {Application.identifier}:{Application.version} {Application.platform}");
			builder.AppendLine($"Device : {SystemInfo.deviceModel}, {SystemInfo.deviceName}, {SystemInfo.deviceType}");
			builder.AppendLine($"Battery : {SystemInfo.batteryStatus}, {SystemInfo.batteryLevel:0.00}");
			builder.AppendLine($"Processor : {SystemInfo.processorType}, {SystemInfo.processorCount}, {SystemInfo.processorFrequency}");
			builder.AppendLine($"Graphics : {SystemInfo.graphicsDeviceName}, {SystemInfo.graphicsDeviceType}, " +
			                   $"{SystemInfo.graphicsDeviceVendor}, {SystemInfo.graphicsDeviceVersion}, " +
			                   $"GMEM : {SystemInfo.graphicsMemorySize}, SM{SystemInfo.graphicsShaderLevel}");

			builder.AppendLine($"OS : {SystemInfo.operatingSystem}, MEM : {SystemInfo.systemMemorySize}, {SystemInfo.operatingSystemFamily}");
			builder.AppendLine("UsesReversedZBuffer : " + SystemInfo.usesReversedZBuffer);
			builder.Append($"NPOT support : {SystemInfo.npotSupport}, Instancing support : {SystemInfo.supportsInstancing}, Texture Size : {SystemInfo.maxTextureSize}, " +
			                   $"Compute : {SystemInfo.supportsComputeShaders}");
			Debug.LogWarning(builder.ToString());
#endif
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		public static void OnSceneLoaded() {
			CSharpServiceManager.InitializeServiceGameObject();
			CSharpServiceManager.Register(new NetworkService());
			CSharpServiceManager.Register(new GlobalCoroutineRunnerService());

			var mode = GameSystem.Get().SystemSetting.GetString("GAME", "Mode");
			if( mode != "Shipping" ) {
				/*using( var assetRef = AssetService.Get().Load<GameObject>("Console.prefab") ) {
					var go = assetRef.Instantiate();
					CSharpServiceManager.Register(go.GetComponent<InGameConsole>());
				}*/
			}

			Application.targetFrameRate = -1;
			var maxInstantiateDuration = GameSystem.Get().SystemSetting.GetDouble("GAME", "MaxInstantiateDuration");
			AssetService.Get().AfterSceneLoaded((float) maxInstantiateDuration);
		}
	}
}