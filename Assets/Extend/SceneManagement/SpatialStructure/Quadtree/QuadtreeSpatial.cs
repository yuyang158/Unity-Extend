using Extend.Common;
using UnityEngine;

namespace Extend.SceneManagement.SpatialStructure.Quadtree {
	public class QuadtreeSpatial : SpatialAbstract {
		private QuadtreeNode m_root;

		[SerializeField]
		private DrawGizmoMode m_gizmoMode = DrawGizmoMode.Leaf;

		private readonly Plane[] m_frustumPlanes = new Plane[6];

		private void Awake() {
			for( int i = 0; i < m_frustumPlanes.Length; i++ ) {
				m_frustumPlanes[i] = new Plane();
			}
		}

		public override void CullVisible(Camera renderCamera) {
			GeometryUtility.CalculateFrustumPlanes(renderCamera, m_frustumPlanes);
			m_root?.Cull(m_frustumPlanes);
		}

		private int m_totalRendererCount;

		[Button(ButtonSize.Medium)]
		public override void Build() {
			var renderers = GetComponentsInChildren<Renderer>();
			m_totalRendererCount = renderers.Length;
			m_root = new QuadtreeNode(renderers, 0);
		}

		public override int RendererCount => m_totalRendererCount;

		private void OnDrawGizmosSelected() {
			m_root?.DrawGizmo(m_gizmoMode, 0);
		}
	}
}