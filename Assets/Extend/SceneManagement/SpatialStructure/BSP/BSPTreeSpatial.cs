using Extend.Common;
using Extend.SceneManagement.Jobs;
using UnityEngine;

namespace Extend.SceneManagement.SpatialStructure.BSP {
	public class BSPTreeSpatial : SpatialAbstract {
		private BSPTreeNode m_root;
		private int m_rendererCount;

		[SerializeField]
		private DrawGizmoMode m_gizmoMode = DrawGizmoMode.Leaf;

		public override void CullVisible(Plane[] frustumPlanes) {
			m_root?.Cull(frustumPlanes);
		}

		[Button(ButtonSize.Medium)]
		public override void Build(DrawJobSchedule jobSchedule) {
			base.Build(jobSchedule);
			m_root = new BSPTreeNode(jobSchedule, JobSchedule.Instances.ToArray(), 0);
			JobSchedule.AfterBuild();
		}

		public override int RendererCount => m_rendererCount;

		private void OnDrawGizmosSelected() {
			m_root?.DrawGizmo(m_gizmoMode, 0, m_onlyVisibleGizmo);
		}
	}
}