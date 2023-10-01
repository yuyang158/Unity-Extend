using System.Collections.Generic;
using UnityEngine;

namespace Extend.DoTween {
	public static class MaterialPropertyBlockPool {
		private static readonly Dictionary<Renderer, MaterialPropertyBlock> m_blocks = new Dictionary<Renderer, MaterialPropertyBlock>();

		public static MaterialPropertyBlock RequestBlock(Renderer renderer) {
			if( !renderer ) {
				return null;
			}
			if( !m_blocks.TryGetValue(renderer, out var block) ) {
				block = new MaterialPropertyBlock();
				renderer.GetPropertyBlock(block);
				m_blocks.Add(renderer, block);
				renderer.SetPropertyBlock(block);
			}

			return block;
		}

		public static void GiveUpRenderer(Renderer renderer) {
			m_blocks.Remove(renderer);
		}
	}
}
