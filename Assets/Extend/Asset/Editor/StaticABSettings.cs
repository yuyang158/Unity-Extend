using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Extend.Asset.Editor {
	[Serializable]
	public class StaticABSetting {
		public enum Operation {
			ALL_IN_ONE,
			STAY_RESOURCES,
			EACH_FOLDER_ONE,
			EACH_A_AB
		}
		
		public DefaultAsset FolderPath;
		public Operation Op;

		public SpecialBundleLoadLogic[] UnloadStrategies;

		public string Path => FolderPath ? AssetDatabase.GetAssetPath( FolderPath ) : string.Empty;
	}

	[Serializable]
	public class SpecialBundleLoadLogic {
		public string BundleName;
		public BundleUnloadStrategy UnloadStrategy = BundleUnloadStrategy.Normal;
	}
	
	public class StaticABSettings : ScriptableObject {
		public StaticABSetting[] Settings;
		public Object[] ExtraDependencyAssets;

		public SceneAsset[] Scenes;

		public bool ContainExtraObject(Object obj) {
			return Array.Find(ExtraDependencyAssets, dep => dep == obj);
		}
	}
}