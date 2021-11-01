using System;
using Extend.Common;
using UnityEngine;

namespace Extend.SceneManagement.SpatialStructure {
	public abstract class TreeNode {
		protected Bounds m_bounds;
		protected TreeNode[] m_children;
		private bool m_visible = true;
		protected Renderer[] m_renderers;

		protected abstract bool HasChildren { get; }
		
		public void SetVisible(bool visible) {
			if( m_visible == visible ) {
				foreach( var child in m_children ) {
					child?.SetVisible(m_visible);
				}
				return;
			}

			m_visible = visible;
			if( m_visible ) {
				if( HasChildren ) {
					foreach( var child in m_children ) {
						child?.SetVisible(m_visible);
					}
				}
				else {
					foreach( var renderer in m_renderers ) {
						renderer.enabled = m_visible;
					}
				}
			}
			else {
				if( m_renderers == null ) {
					foreach( var child in m_children ) {
						child?.SetVisible(m_visible);
					}
				}
				else {
					foreach( var renderer in m_renderers ) {
						renderer.enabled = m_visible;
					}
				}
			}
		}
		
		
		public void Cull(Plane[] frustumPlanes) {
			StatService.Get().Increase(StatService.StatName.CULL_PROCESS, 1);
			if( GeometryUtility.TestPlanesAABB(frustumPlanes, m_bounds) ) {
				if( HasChildren ) {
					m_visible = true;
					foreach( var child in m_children ) {
						child?.Cull(frustumPlanes);
					}
				}
				else {
					SetVisible(true);
				}
			}
			else {
				SetVisible(false);
			}
		}

		public void DrawGizmo(DrawGizmoMode mode, int deep) {
			switch( mode ) {
				case DrawGizmoMode.All:
					Gizmos.DrawWireCube(m_bounds.center, m_bounds.size);
					break;
				case DrawGizmoMode.Leaf:
					if( !HasChildren ) {
						Gizmos.DrawWireCube(m_bounds.center, m_bounds.size);
					}
					break;
				case DrawGizmoMode.DeepGreater3:
					if( deep > 3 || !HasChildren ) {
						Gizmos.DrawWireCube(m_bounds.center, m_bounds.size);
					}
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
			}
			foreach( var child in m_children ) {
				child?.DrawGizmo(mode, deep + 1);
			}
		}
	}
}