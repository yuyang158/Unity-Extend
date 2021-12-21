using Extend.Common;
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

		private int m_totalRendererCount;

		[Button(ButtonSize.Medium)]
		public override void Build(DrawJobSchedule jobSchedule) {
			base.Build(jobSchedule);
			m_root = new QuadtreeNode(jobSchedule, JobSchedule.Instances.ToArray(), 0);
			JobSchedule.AfterBuild();
		}

		public override int RendererCount => m_totalRendererCount;

		private void OnDrawGizmosSelected() {
			m_root?.DrawGizmo(m_gizmoMode, 0, m_onlyVisibleGizmo);
		}
	}
}