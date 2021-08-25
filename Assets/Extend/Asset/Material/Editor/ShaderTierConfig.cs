using System;
using UnityEngine;

namespace Extend.Asset.Material.Editor {
	public class ShaderTierConfig : ScriptableObject {
		[Serializable]
		public class ShaderTierDisallowKeywords {
			public string[] Keywords;
		}
		
		[Serializable]
		public class ShaderTier {
			public Shader Shader;

			public ShaderTierDisallowKeywords DisallowKeywords;
		}

		public ShaderTier[] Tiers;
	}
}