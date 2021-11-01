using System.Collections.Generic;
using Extend.Common;
using UnityEngine;

namespace Extend.SceneManagement.SpatialStructure.BSP {
	public class BSPTreeNode : TreeNode {
		public BSPTreeNode(Renderer[] renderers, int deep) {
			m_children = new TreeNode[2];
			if( deep > 3 ) {
				m_renderers = renderers;
			}
			
			m_bounds = SpatialAbstract.CalculationBounds(renderers);
			if( deep > 8 || m_bounds.size.x < 5 || m_bounds.size.z < 5 ) {
				m_renderers = renderers;
				return;
			}
			
			var leftBound = new Bounds();
			leftBound.SetMinMax(m_bounds.min, m_bounds.extents.x > m_bounds.extents.z
				? new Vector3(m_bounds.center.x, m_bounds.max.y, m_bounds.max.z)
				: new Vector3(m_bounds.max.x, m_bounds.max.y, m_bounds.center.z));

			List<Renderer> rendererList = new List<Renderer>(renderers);
			var overlappedRenderers = new List<Renderer>(renderers.Length);
			for( int i = 0; i < rendererList.Count; ) {
				var renderer = rendererList[i];
				if( leftBound.Contains(renderer.transform.position) ) {
					rendererList.RemoveSwapAt(i);
					overlappedRenderers.Add(renderer);
				}
				else {
					i++;
				}
			}

			if( rendererList.Count == 0 ) {
				m_renderers = renderers;
				return;
			}

			m_children[0] = new BSPTreeNode(overlappedRenderers.ToArray(), deep + 1);
			m_children[1] = new BSPTreeNode(rendererList.ToArray(), deep + 1);
		}

		protected override bool HasChildren => m_children[0] != null || m_children[1] != null;
	}
}