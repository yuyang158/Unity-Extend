using System;
using System.Collections;
using System.IO;
using System.Threading;
using Extend.Common;
using UnityEngine;
using UnityEngine.Networking;

namespace Extend.Asset {
	public static class FileLoader {
#if UNITY_IOS
		const string platform = "iOS";
#elif UNITY_ANDROID
		const string platform = "Android";
#else
		const string platform = "StandaloneWindows64";
#endif

		private static readonly string STREAMING_ASSET_BUNDLE_PATH = Path.Combine(Application.streamingAssetsPath, "ABBuild", platform);
		private static readonly string PERSISTENT_ASSET_BUNDLE_PATH = Path.Combine(Application.persistentDataPath, "ABBuild", platform);

		private static string DetermineAssetLocation(string path, out bool underPersistent) {
			var assetPath = Path.Combine(Application.persistentDataPath, path);
			underPersistent = File.Exists(assetPath);
			if( !underPersistent ) {
				assetPath = Path.Combine(Application.streamingAssetsPath, path);
			}

			return assetPath;
		}

		public static string DetermineBundleLocation(string path) {
			return DetermineBundleLocation(path, out _);
		}

		public static string DetermineBundleLocation(string path, out bool underPersistent) {
			var assetPath = Path.Combine(PERSISTENT_ASSET_BUNDLE_PATH, path);
			underPersistent = File.Exists(assetPath);
			return underPersistent ? assetPath : Path.Combine(STREAMING_ASSET_BUNDLE_PATH, path);
		}

		public static void LoadFileAsync(string path, Action<byte[]> callback) {
			var assetPath = DetermineAssetLocation(path, out var underPersistent);
			if( underPersistent || Application.platform != RuntimePlatform.Android || Application.isEditor ) {
				ThreadPool.QueueUserWorkItem(_ => {
					if( File.Exists(assetPath) ) {
						var bytes = File.ReadAllBytes(assetPath);
						UnityMainThreadDispatcher.Instance().Enqueue(() => { callback(bytes); });
					}
					else {
						Debug.LogError($"File not exist : {assetPath}.");
						UnityMainThreadDispatcher.Instance().Enqueue(() => { callback(null); });
					}
				});
				return;
			}

			var service = CSharpServiceManager.Get<GlobalCoroutineRunnerService>(CSharpServiceManager.ServiceType.COROUTINE_SERVICE);
			service.StartCoroutine(WebRequestFile(assetPath, callback));
		}

		public static Stream LoadFileSync(string path, bool autoDetectPath = true) {
			var assetPath = DetermineAssetLocation(path, out var underPersistent);
			if( !autoDetectPath ) {
				underPersistent = false;
			}
			if( underPersistent || Application.platform != RuntimePlatform.Android || Application.isEditor ) {
				return new FileStream(assetPath, FileMode.Open, FileAccess.Read);
			}

			var uwr = UnityWebRequest.Get(assetPath);
			uwr.SendWebRequest();
			while( !uwr.isDone ) {
				Thread.Sleep(0);
			}

			var stream = new MemoryStream(uwr.downloadHandler.data);
			uwr.Dispose();
			return stream;
		}

		private static IEnumerator WebRequestFile(string assetPath, Action<byte[]> callback) {
			using( var uwr = UnityWebRequest.Get(assetPath) ) {
				yield return uwr.SendWebRequest();
				callback(uwr.downloadHandler.data);
			}
		}

		public static Stream LoadBundleFileSync(string path) {
			var assetPath = DetermineBundleLocation(path, out var underPersistent);
			if( underPersistent || Application.platform != RuntimePlatform.Android ) {
				return new FileStream(assetPath, FileMode.Open, FileAccess.Read);
			}

			var uwr = UnityWebRequest.Get(assetPath);
			uwr.SendWebRequest();
			while( !uwr.isDone ) {
				Thread.Sleep(1);
			}

			var stream = new MemoryStream(uwr.downloadHandler.data);
			uwr.Dispose();
			return stream;
		}
	}
}