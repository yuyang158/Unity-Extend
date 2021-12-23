﻿using Extend.Common;
using Extend.SceneManagement.Jobs;
using UnityEngine;

namespace Extend.SceneManagement.SpatialStructure.Quadtree {
	public class QuadtreeSpatial : SpatialAbstract {
		private QuadtreeNode m_root;

		[SerializeField]
		private DrawGizmoMode m_gizmoMode = DrawGizmoMode.Leaf;

		public override void CullVisible(Plane[] frustumPlanes) {
			m_root?.Cull(frustumPlanes);
		}

		[Button(ButtonSize.Medium)]
		public override void Build(DrawJobSchedule jobSchedule) {
			base.Build(jobSchedule);
			m_totalRendererCount = JobSchedule.Instances.Count;
			m_root = new QuadtreeNode(jobSchedule, JobSchedule.Instances.ToArray(), 0);
			JobSchedule.AfterBuild();
		}

		private void OnDrawGizmosSelected() {
			m_root?.DrawGizmo(m_gizmoMode, 0, m_onlyVisibleGizmo);
		}
	}
}