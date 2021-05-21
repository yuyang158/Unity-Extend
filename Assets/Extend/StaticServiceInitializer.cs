using System.Text;
using DG.Tweening;
using Extend.Asset;
using Extend.Common;
using Extend.DebugUtil;
using Extend.LuaUtil;
using Extend.Network;
using Extend.UI.i18n;
using Extend.Render;
using UnityEngine;

namespace Extend {
	internal static class StaticServiceInitializer {
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void OnInit() {
			Application.runInBackground = true;
			DOTween.Init(false, true, LogBehaviour.Default);

			CSharpServiceManager.Initialize();
			CSharpServiceManager.Register(new ErrorLogToFile());
			CSharpServiceManager.Register(new GlobalCoroutineRunnerService());
			CSharpServiceManager.Register(new StatService());
#if UNITY_DEBUG
			CSharpServiceManager.Register(new AssetFullStatService());
#endif
			CSharpServiceManager.Register(new AssetService());
			CSharpServiceManager.Register(new GameSystem());
			CSharpServiceManager.Register(new RenderFeatureService());
			CSharpServiceManager.Register(new SpriteAssetService());
			CSharpServiceManager.Register(new I18nService());
			CSharpServiceManager.Register(new LuaVM());
			CSharpServiceManager.Register(new TickService());

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
			builder.Append($"NPOT : {SystemInfo.npotSupport}, INSTANCING : {SystemInfo.supportsInstancing}, Texture Size : {SystemInfo.maxTextureSize}, " +
			                   $"Compute : {SystemInfo.supportsComputeShaders}");
			Debug.LogWarning(builder.ToString());
#endif
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		public static void OnSceneLoaded() {
			CSharpServiceManager.Register(new NetworkService());

			var mode = GameSystem.Get().SystemSetting.GetString("GAME", "Mode");
			if( mode != "Shipping" ) {
				using( var assetRef = AssetService.Get().Load("Console", typeof(GameObject)) ) {
					var go = assetRef.Instantiate();
					CSharpServiceManager.Register(go.GetComponent<InGameConsole>());
				}
			}

			Application.targetFrameRate = 30;
			var maxInstantiateDuration = GameSystem.Get().SystemSetting.GetDouble("GAME", "MaxInstantiateDuration");
			AssetService.Get().AfterSceneLoaded((float)maxInstantiateDuration);
			var lua = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			lua.StartUp();
		}
	}
}