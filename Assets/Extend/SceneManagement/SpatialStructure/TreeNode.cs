using System;
using Extend.Common;
using Extend.SceneManagement.Jobs;
using UnityEngine;

namespace Extend.SceneManagement.SpatialStructure {
	public struct NodeIndexInstance {
		public int MaterialIndex;
		public int InstanceIndex;
	}
	
	public abstract class TreeNode {
		protected Bounds m_bounds;
		protected TreeNode[] m_children;
		private bool m_visible;
		private NodeIndexInstance[] m_instances;

		protected abstract bool HasChildren { get; }
		private readonly DrawJobSchedule m_jobSchedule;

		protected TreeNode(DrawJobSchedule jobSchedule) {
			m_jobSchedule = jobSchedule;
		}

		protected void SetInstances(DrawInstance[] instances) {
			m_instances = new NodeIndexInstance[instances.Length];
			for( int i = 0; i < instances.Length; i++ ) {
				var instance = instances[i];
				m_instances[i] = new NodeIndexInstance {
					MaterialIndex = instance.MaterialIndex,
					InstanceIndex = instance.Index
				};
			}
		}

		private void SetVisible(bool visible) {
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
					foreach( var instance in m_instances ) {
						var meshMaterial = m_jobSchedule.GetMeshMaterial(instance.MaterialIndex);
						meshMaterial.SetVisible(instance.InstanceIndex, m_visible);
					}
				}
			}
			else {
				if( m_instances == null ) {
					foreach( var child in m_children ) {
						child?.SetVisible(m_visible);
					}
				}
				else {
					foreach( var instance in m_instances ) {
						var meshMaterial = m_jobSchedule.GetMeshMaterial(instance.MaterialIndex);
						meshMaterial.SetVisible(instance.InstanceIndex, m_visible);
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

		public void DrawGizmo(DrawGizmoMode mode, int deep, bool onlyVisibleGizmo) {
			if( onlyVisibleGizmo ) {
				var planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
				if( !GeometryUtility.TestPlanesAABB(planes, m_bounds) ) {
					return;
				}
			}
			
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
				child?.DrawGizmo(mode, deep + 1, onlyVisibleGizmo);
			}
		}
	}
}