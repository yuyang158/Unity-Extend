using System;
using UnityEngine;

namespace Extend.Asset.Material.Editor {
	public class ShaderTierConfig : ScriptableObject {
		[Serializable]
		public class ShaderTier {
			public string[] Keywords;
		}

		[Serializable]
		public class Shader3TierCombine {
			public Shader Shader;
			public ShaderTier[] Tiers;
		}

		public Shader3TierCombine[] Shaders;

		private void OnEnable() {
			Debug.Log("LOADED");
		}
	}
}