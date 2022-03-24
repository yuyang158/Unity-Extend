using System.Collections.Generic;
using Extend.Common;
using Extend.SceneManagement.Jobs;
using UnityEngine;

namespace Extend.SceneManagement.SpatialStructure.Quadtree {
	public class QuadtreeNode : TreeNode {
		private const float MIN_BOUNDS_SIZE = 5f;

		public QuadtreeNode(DrawJobSchedule jobSchedule, DrawInstance[] instances, int deep)
			: base(jobSchedule) {
			m_children = new TreeNode[4];
			if( instances.Length == 1 ) {
				SetInstances(instances);
				m_bounds = instances[0].Bounds;
				return;
			}

			m_bounds = SpatialAbstract.CalculationBounds(instances);
			var size = m_bounds.size;
			if( deep > 7 || size.x < MIN_BOUNDS_SIZE || size.z < MIN_BOUNDS_SIZE ) {
				SetInstances(instances);
				return;
			}

			var subBounds = GetSubBounds(m_bounds);
			var biggerRenderers = new List<DrawInstance>(instances.Length);
			var allRenderers = new List<DrawInstance>(instances);
			var overlappedRenderers = new List<DrawInstance>(instances.Length);
			for( int i = 0; i < subBounds.Length; i++ ) {
				var subBound = subBounds[i];
				for( int j = 0; j < allRenderers.Count; ) {
					var renderer = allRenderers[j];
					if( renderer.Bounds.extents.magnitude > subBound.extents.magnitude ) {
						allRenderers.RemoveSwapAt(j);
						biggerRenderers.Add(renderer);
						continue;
					}

					if( renderer.Bounds.Intersects(subBound) ) {
						allRenderers.RemoveSwapAt(j);
						overlappedRenderers.Add(renderer);
					}
					else {
						j++;
					}
				}

				if( overlappedRenderers.Count > 0 ) {
					m_children[i] = new QuadtreeNode(jobSchedule, overlappedRenderers.ToArray(), deep + 1);
				}

				overlappedRenderers.Clear();

				if( allRenderers.Count == 0 ) {
					break;
				}
			}

			SetInstances(biggerRenderers.ToArray());
		}

		protected override bool HasChildren => m_children[0] != null || m_children[1] != null || m_children[2] != null || m_children[3] != null;

		private static Bounds[] GetSubBounds(Bounds parent) {
			var bounds = new Bounds[4];
			bounds[0] = new Bounds();
			var min = parent.min;
			var max = new Vector3(parent.center.x, parent.max.y, parent.center.z);
			bounds[0].SetMinMax(min, max);

			bounds[1] = new Bounds();
			min = new Vector3(parent.min.x, parent.min.y, parent.center.z);
			max = new Vector3(parent.center.x, parent.max.y, parent.max.z);
			bounds[1].SetMinMax(min, max);

			bounds[2] = new Bounds();
			min = new Vector3(parent.center.x, parent.min.y, parent.min.z);
			max = new Vector3(parent.max.x, parent.max.y, parent.center.z);
			bounds[2].SetMinMax(min, max);

			bounds[3] = new Bounds();
			min = new Vector3(parent.center.x, parent.min.y, parent.center.z);
			max = parent.max;
			bounds[3].SetMinMax(min, max);

			return bounds;
		}
	}
}