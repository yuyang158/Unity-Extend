using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Extend.Common;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Extend.Asset.AssetProvider {
	public class ResourcesLoadProvider : AssetLoadProvider {
		private static readonly WaitForSeconds DEBUG_WAIT = new WaitForSeconds(0.2f);
		private readonly Dictionary<string, string> m_extraDependencies = new Dictionary<string, string>();
		private const string RESOURCES_FOLDER_NAME = "Assets/Resources";

		private bool ConvertPath(ref string path) {
			if(m_extraDependencies.TryGetValue(path, out var fullPath)) {
				path = fullPath;
				return true;
			}
			return false;
		}

		private IEnumerator SimulateDelayLoad(AssetAsyncLoadHandle loadHandle, Type typ) {
			yield return DEBUG_WAIT;
			var path = loadHandle.Location;
			if(ConvertPath(ref path) || loadHandle.Location.StartsWith("Assets")) {
				loadHandle.Location = path;
				#if UNITY_EDITOR
				var unityObject = UnityEditor.AssetDatabase.LoadAssetAtPath(loadHandle.Location, typ);
				loadHandle.Asset.SetAsset(unityObject, null);
				#endif
			}
			else {
				var req = Resources.LoadAsync(loadHandle.Location, typ);
				req.completed += operation => {
					if( !req.asset ) {
						Debug.LogError("Load fail for : " + loadHandle.Location);
					}
					loadHandle.Asset.SetAsset(operation.isDone ? req.asset : null, null);
				};
			}
		}

		private static IEnumerator SimulateDelayLoadScene(AssetAsyncLoadHandle loadHandle, bool add)
		{
			yield return DEBUG_WAIT;
			var asyncOperation = SceneManager.LoadSceneAsync(loadHandle.Location + ".unity", 
				add ? LoadSceneMode.Additive : LoadSceneMode.Single);
			while (!asyncOperation.isDone)
			{
				yield return null;
			}
			loadHandle.Complete();
		}

		public override void Initialize() {
			#if UNITY_EDITOR
			var abSetting = UnityEditor.AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/Extend/Asset/Editor/settings.asset");
			var extraField = abSetting.GetType().GetField("ExtraDependencyAssets");
			var dependencies = extraField.GetValue(abSetting) as Object[];
			foreach (var dep in dependencies) {
				var path = UnityEditor.AssetDatabase.GetAssetPath(dep);
				m_extraDependencies.Add(path[..path.LastIndexOf('.')], path);
			}
			#endif
		}

		public override void ProvideAsync(AssetAsyncLoadHandle loadHandle, Type typ) {
			var service = CSharpServiceManager.Get<GlobalCoroutineRunnerService>(CSharpServiceManager.ServiceType.COROUTINE_SERVICE);
			service.StartCoroutine(SimulateDelayLoad(loadHandle, typ));
		}

		public override AssetReference Provide(string path, AssetContainer container, Type typ) {
			var asset = ProvideAsset(path, container, typ);
			return new AssetReference(asset);
		}

		public override void ProvideSceneAsync(AssetAsyncLoadHandle loadHandle, bool add)
		{
			var service = CSharpServiceManager.Get<GlobalCoroutineRunnerService>(CSharpServiceManager.ServiceType.COROUTINE_SERVICE);
			service.StartCoroutine(SimulateDelayLoadScene(loadHandle, add));
		}

		public override void ProvideScene(string path, AssetContainer container, bool add)
		{
			SceneManager.LoadScene(path + ".unity",
				add ? LoadSceneMode.Additive : LoadSceneMode.Single);
		}

		public override bool Exist(string path) {
			if(ConvertPath(ref path) || path.StartsWith("Assets") ) {
				#if UNITY_EDITOR
				return UnityEditor.AssetDatabase.AssetPathToGUID(path) != null;
				#else
				return false;
				#endif
			}
			else {
				var unityObject = Resources.Load<Object>(path);
				return unityObject != null;
			}
		}

		private AssetInstance ProvideAsset(string path, AssetContainer container, Type typ) {
			var hash = AssetInstance.GenerateHash(path);
			if( container.TryGetAsset(hash) is AssetInstance {IsFinished: true} asset ) {
				return asset;
			}

			asset = typ == typeof(GameObject) ? new PrefabAssetInstance(path) : new AssetInstance(path);
			Object unityObject;
			if(ConvertPath(ref path) || path.StartsWith("Assets")) {
				#if UNITY_EDITOR
				unityObject = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typ);
				#else
				unityObject = null;
				#endif
			}
			else {
				unityObject = Resources.Load(path, typ);
			}

			asset.SetAsset(unityObject, null);
			return asset;
		}

		internal override AssetInstance ProvideAssetWithGUID<T>(string guid, AssetContainer container, out string path) {
#if UNITY_EDITOR
			path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
			if( string.IsNullOrEmpty(path) ) {
				Debug.LogWarning($"Missing asset for guid {guid}");
				return null;
			}
			return ProvideAsset(path, container, typeof(T));
#else
			path = string.Empty;
			return null;
#endif
		}

		internal override string ConvertGUID2Path(string guid) {
#if UNITY_EDITOR
			return UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
#else
			return null;
#endif
		}
	}
}