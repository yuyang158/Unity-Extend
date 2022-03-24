using Extend.Common;
using Extend.SceneManagement.Culling;
using UnityEngine;

namespace Extend.SceneManagement.SpatialStructure.Quadtree {
	public class QuadtreeSpatial : SpatialAbstract {
		private QuadtreeNode m_root;

		[SerializeField]
		private DrawGizmoMode m_gizmoMode = DrawGizmoMode.Leaf;

		public override void CullVisible(CullMethodBase cullMethod) {
			m_root?.Cull(cullMethod);
		}

		[Button(ButtonSize.Medium)]
		public override void Build() {
			var specialSceneElements = BuildCollect();
			m_root = new QuadtreeNode(JobSchedule, JobSchedule.Instances.ToArray(), 0);
			JobSchedule.AfterBuild();
			m_root.ProcessSpecialSceneElement(specialSceneElements);
		}

		private void OnDrawGizmosSelected() {
			m_root?.DrawGizmo(m_gizmoMode, 0, m_onlyVisibleGizmo);
		}
	}
}