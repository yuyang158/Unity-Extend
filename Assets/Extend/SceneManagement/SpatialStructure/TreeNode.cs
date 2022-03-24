using System;
using System.Collections.Generic;
using Extend.Common;
using Extend.SceneManagement.Culling;
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
		private SpecialSceneElement[] m_specialSceneElements;

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
				return;
			}

			m_visible = visible;
			foreach( var instance in m_instances ) {
				var meshMaterial = m_jobSchedule.GetMeshMaterial(instance.MaterialIndex);
				meshMaterial.SetVisible(instance.InstanceIndex, m_visible);
			}

			if( m_specialSceneElements != null ) {
				foreach( var element in m_specialSceneElements ) {
					element.SetVisible(visible);
				}
			}

			if( !m_visible ) {
				foreach( var child in m_children ) {
					child?.SetVisible(m_visible);
				}
			}
		}

		public void Cull(CullMethodBase cullMethod) {
			StatService.Get().Increase(StatService.StatName.CULL_PROCESS, 1);
			if( cullMethod.Cull(m_bounds) ) {
				foreach( var child in m_children ) {
					child?.Cull(cullMethod);
				}
				SetVisible(true);
			}
			else {
				SetVisible(false);
			}
		}

		public void ProcessSpecialSceneElement(List<SpecialSceneElement> specialElements) {
			List<SpecialSceneElement> childElements = new List<SpecialSceneElement>(specialElements.Count);
			foreach( var child in m_children ) {
				if( child == null ) {
					continue;
				}
				for( int i = 0; i < specialElements.Count; ) {
					var element = specialElements[i];
					var bound = child.m_bounds;
					if( bound.Contains(element.Bounds.min) && bound.Contains(element.Bounds.max) ) {
						specialElements.RemoveAt(i);
						childElements.Add(element);
					}
					else {
						i++;
					}
				}
				if( childElements.Count == 0 ) {
					continue;
				}

				child.ProcessSpecialSceneElement(childElements);
				childElements.Clear();
			}

			if( specialElements.Count > 0 ) {
				m_specialSceneElements = specialElements.ToArray();
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