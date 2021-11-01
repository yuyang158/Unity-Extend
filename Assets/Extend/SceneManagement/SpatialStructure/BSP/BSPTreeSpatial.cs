using Extend.Common;
using UnityEngine;

namespace Extend.SceneManagement.SpatialStructure.BSP {
	public class BSPTreeSpatial : SpatialAbstract {
		private BSPTreeNode m_root;
		private int m_rendererCount;

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

		[Button(ButtonSize.Medium)]
		public override void Build() {
			var renderers = GetComponentsInChildren<Renderer>();
			m_rendererCount = renderers.Length;
			m_root = new BSPTreeNode(renderers, 0);
		}

		public override int RendererCount => m_rendererCount;
		
		private void OnDrawGizmosSelected() {
			m_root?.DrawGizmo(m_gizmoMode, 0);
		}
	}
}