using System.Collections.Generic;
using UnityEngine;

namespace Extend.DoTween {
	public static class MaterialPropertyBlockPool {
		private static readonly Dictionary<Renderer, MaterialPropertyBlock> m_blocks = new();
		private static readonly Stack<MaterialPropertyBlock> m_blockPool = new();

		public static MaterialPropertyBlock RequestBlock(Renderer renderer) {
			if( !renderer ) {
				return null;
			}
			if( !m_blocks.TryGetValue(renderer, out var block) ) {
				block = m_blockPool.Count > 0 ? m_blockPool.Pop() : new MaterialPropertyBlock();
				renderer.GetPropertyBlock(block);
				m_blocks.Add(renderer, block);
				renderer.SetPropertyBlock(block);
			}

			return block;
		}

		public static void GiveUpRenderer(Renderer renderer) {
			if( m_blocks.Remove(renderer, out var block) ) {
				m_blockPool.Push(block);
			}
		}
	}
}
