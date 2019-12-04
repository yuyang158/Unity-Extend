using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Extend.AssetService.Editor {
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

		public string Path => FolderPath ? AssetDatabase.GetAssetPath( FolderPath ) : string.Empty;
	}
	
	public class StaticABSettings : ScriptableObject {
		public DefaultAsset SpriteAtlasFolder;
		
		public List<StaticABSetting> Settings;
	}
}