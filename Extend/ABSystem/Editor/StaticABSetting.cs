using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ABSystem.Editor {
	[Serializable]
	public class StaticABSetting {
		public enum Operation {
			ALL_IN_ONE,
			STAY_RESOURCES
		}
		
		public DefaultAsset FolderPath;
		public Operation Op;
	}

	public class StaticABSettings : ScriptableObject {
		public List<StaticABSetting> Settings;
	}
}