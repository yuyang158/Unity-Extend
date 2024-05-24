using System;
using System.Collections.Generic;
using System.Linq;
using Extend.SceneManagement.Culling;
using Extend.SceneManagement.Jobs;
using UnityEngine;

namespace Extend.SceneManagement.SpatialStructure {
	public enum DrawGizmoMode {
		All,
		Leaf,
		DeepGreater3
	}

	[Flags]
	public enum SpecialControlFlag {
		ParticleSystem = 1,
		Light = 2,
		VisualEffect = 4
	}

	public class SpecialSceneElement {
		private readonly SpecialControlFlag m_type;
		public readonly Bounds Bounds;
		private readonly Component m_component;
		public SpecialSceneElement(SpecialControlFlag type, Bounds bounds, Component component) {
			m_type = type;
			Bounds = bounds;
			m_component = component;
			SetVisible(false);
		}

		public void SetVisible(bool visible) {
			switch( m_type ) {
				case SpecialControlFlag.ParticleSystem:
					var ps = (ParticleSystem)m_component;
					var renderer = ps.GetComponent<ParticleSystemRenderer>();
					if( visible ) {
						ps.Play();
					}
					else {
						ps.Stop();
					}
					renderer.enabled = visible;
					break;
				case SpecialControlFlag.Light:
					((Light)m_component).enabled = visible;
					break;
				case SpecialControlFlag.VisualEffect:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}

	public abstract class SpatialAbstract : MonoBehaviour {
		[SerializeField]
		protected bool m_onlyVisibleGizmo;
		[SerializeField]
		private SpecialControlFlag m_control;

		public int RendererCount { get; private set; }

		public DrawJobSchedule JobSchedule { get; private set; }

		private void Awake() {
			JobSchedule = new DrawJobSchedule();
		}

		private void OnDestroy() {
			JobSchedule.Dispose();
		}

		public abstract void CullVisible(CullMethodBase cullMethod);

		public abstract void Build();

		public void SetVisible(int combinedId, bool visible) {
			DrawJobSchedule.SplitCombineID(combinedId, out var materialIndex, out var instanceIndex);
			var meshMaterial = JobSchedule.GetMeshMaterial(materialIndex);
			meshMaterial.SetVisible(instanceIndex, visible);
		}

		protected List<SpecialSceneElement> BuildCollect() {
			JobSchedule.Prepare();
			var meshRenderers = GetComponentsInChildren<MeshRenderer>();
			foreach( var meshRenderer in meshRenderers ) {
				var marker = meshRenderer.GetComponent<SpatialIgnoreMarker>();
				if( marker ) {
					continue;
				}
				
				var meshFilter = meshRenderer.GetComponent<MeshFilter>();
				var sharedMesh = meshFilter.sharedMesh;
				JobSchedule.ConvertRenderer(sharedMesh, meshRenderer);
				meshRenderer.forceRenderingOff = true;
			}

			RendererCount = JobSchedule.Instances.Count;
			List<SpecialSceneElement> sceneElements = new List<SpecialSceneElement>(256);
			if( ( m_control & SpecialControlFlag.Light ) != 0 ) {
				var lights = GetComponentsInChildren<Light>();
				sceneElements.AddRange(from lightComponent in lights 
					where lightComponent.type != LightType.Area && lightComponent.type != LightType.Directional 
					let bounds = new Bounds(lightComponent.transform.position, new Vector3(lightComponent.range, 0, lightComponent.range)) 
					select new SpecialSceneElement(SpecialControlFlag.Light, bounds, lightComponent));
			}

			if( ( m_control & SpecialControlFlag.ParticleSystem ) != 0 ) {
				var pss = GetComponentsInChildren<ParticleSystemRenderer>();
				foreach( var ps in pss ) {
					var extends = ps.bounds.extents;
					extends.y = 0;
					sceneElements.Add(new SpecialSceneElement(SpecialControlFlag.ParticleSystem,
						new Bounds(ps.transform.position, extends), ps.GetComponent<ParticleSystem>()));
				}
			}

			return sceneElements;
		}

		public static Bounds CalculationBounds(DrawInstance[] instances) {
			var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

			foreach( var instance in instances ) {
				var boundsMin = instance.Bounds.min;
				if( min.x > boundsMin.x ) {
					min.x = boundsMin.x;
				}

				if( min.y > boundsMin.y ) {
					min.y = boundsMin.y;
				}

				if( min.z > boundsMin.z ) {
					min.z = boundsMin.z;
				}

				var boundsMax = instance.Bounds.max;
				if( max.x < boundsMax.x ) {
					max.x = boundsMax.x;
				}

				if( max.y < boundsMax.y ) {
					max.y = boundsMax.y;
				}

				if( max.z < boundsMax.z ) {
					max.z = boundsMax.z;
				}
			}

			var bounds = new Bounds();
			bounds.SetMinMax(min, max);
			return bounds;
		}
	}
}