using System;
using System.Collections.Generic;
using Extend.SceneManagement.Jobs;
using UnityEngine;

namespace Extend.SceneManagement.SpatialStructure {
	public enum DrawGizmoMode {
		All,
		Leaf,
		DeepGreater3
	}
	
	public abstract class SpatialAbstract : MonoBehaviour {
		[SerializeField]
		protected bool m_onlyVisibleGizmo;

		public abstract void CullVisible(Plane[] frustumPlanes);

		public DrawJobSchedule JobSchedule { get; private set; }

		private void Awake() {
			JobSchedule = new DrawJobSchedule();
		}

		private void OnDestroy() {
			JobSchedule.Dispose();
		}

		public virtual void Build(DrawJobSchedule jobSchedule) {
			JobSchedule.Prepare();
			var renderers = GetComponentsInChildren<MeshRenderer>();
			foreach( var r in renderers ) {
				var meshFilter = r.GetComponent<MeshFilter>();
				var sharedMesh = meshFilter.sharedMesh;
				JobSchedule.ConvertRenderer(sharedMesh, r);
				r.enabled = false;
				Destroy(r.gameObject);
			}
		}

		public abstract int RendererCount { get; }

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