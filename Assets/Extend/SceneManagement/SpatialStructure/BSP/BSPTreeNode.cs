using System.Collections.Generic;
using Extend.Common;
using Extend.Render;
using Extend.SceneManagement.Jobs;
using UnityEngine;

namespace Extend.SceneManagement.SpatialStructure.BSP {
	public class BSPTreeNode : TreeNode {
		public BSPTreeNode(DrawJobSchedule jobSchedule, DrawInstance[] instances, int deep)
			: base(jobSchedule) {
			m_children = new TreeNode[2];
			if( deep > 3 ) {
				SetInstances(instances);
			}
			
			m_bounds = SpatialAbstract.CalculationBounds(instances);
			if( deep > 8 || m_bounds.size.x < 5 || m_bounds.size.z < 5 ) {
				SetInstances(instances);
				return;
			}
			
			var leftBound = new Bounds();
			leftBound.SetMinMax(m_bounds.min, m_bounds.extents.x > m_bounds.extents.z
				? new Vector3(m_bounds.center.x, m_bounds.max.y, m_bounds.max.z)
				: new Vector3(m_bounds.max.x, m_bounds.max.y, m_bounds.center.z));

			var rendererList = new List<DrawInstance>(instances);
			var overlappedRenderers = new List<DrawInstance>(instances.Length);
			for( int i = 0; i < rendererList.Count; ) {
				var renderer = rendererList[i];
				if( leftBound.Contains(renderer.Bounds.center) ) {
					rendererList.RemoveSwapAt(i);
					overlappedRenderers.Add(renderer);
				}
				else {
					i++;
				}
			}

			if( rendererList.Count == 0 ) {
				SetInstances(instances);
				return;
			}

			m_children[0] = new BSPTreeNode(jobSchedule, overlappedRenderers.ToArray(), deep + 1);
			m_children[1] = new BSPTreeNode(jobSchedule, rendererList.ToArray(), deep + 1);
		}

		protected override bool HasChildren => m_children[0] != null || m_children[1] != null;
	}
}