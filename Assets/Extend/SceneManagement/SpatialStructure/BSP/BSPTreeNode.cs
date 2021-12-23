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
			m_bounds = SpatialAbstract.CalculationBounds(instances);
			if( deep > 8 || m_bounds.size.x < 5 || m_bounds.size.z < 5 ) {
				SetInstances(instances);
				return;
			}
			
			var leftBound = new Bounds();
			leftBound.SetMinMax(m_bounds.min, m_bounds.extents.x > m_bounds.extents.z
				? new Vector3(m_bounds.center.x, m_bounds.max.y, m_bounds.max.z)
				: new Vector3(m_bounds.max.x, m_bounds.max.y, m_bounds.center.z));

			var biggerRenderers = new List<DrawInstance>(instances.Length);
			var allRenderers = new List<DrawInstance>(instances);
			var overlappedRenderers = new List<DrawInstance>(instances.Length);
			for( int i = 0; i < allRenderers.Count; ) {
				var renderer = allRenderers[i];
				if( renderer.Bounds.extents.magnitude > leftBound.extents.magnitude ) {
					allRenderers.RemoveSwapAt(i);
					biggerRenderers.Add(renderer);
					continue;
				}
				if( leftBound.Contains(renderer.Bounds.center) ) {
					allRenderers.RemoveSwapAt(i);
					overlappedRenderers.Add(renderer);
				}
				else {
					i++;
				}
				
				if( allRenderers.Count == 0 ) {
					break;
				}
			}

			if( biggerRenderers.Count > 0 ) {
				SetInstances(biggerRenderers.ToArray());
			}

			m_children[0] = overlappedRenderers.Count > 0 ? new BSPTreeNode(jobSchedule, overlappedRenderers.ToArray(), deep + 1) : null;
			m_children[1] = allRenderers.Count > 0 ? new BSPTreeNode(jobSchedule, allRenderers.ToArray(), deep + 1) : null;
		}

		protected override bool HasChildren => m_children[0] != null || m_children[1] != null;
	}
}