using UnityEngine;

namespace Extend.SceneManagement.SpatialStructure {
	public enum DrawGizmoMode {
		All,
		Leaf,
		DeepGreater3
	}
	
	public abstract class SpatialAbstract : MonoBehaviour {
		public abstract void CullVisible(Camera renderCamera);

		public abstract void Build();
		
		public abstract int RendererCount { get; }
		
		public static Bounds CalculationBounds(Renderer[] renderers) {
			var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

			foreach( var render in renderers ) {
				var boundsMin = render.bounds.min;
				if( min.x > boundsMin.x ) {
					min.x = boundsMin.x;
				}

				if( min.y > boundsMin.y ) {
					min.y = boundsMin.y;
				}
				
				if( min.z > boundsMin.z ) {
					min.z = boundsMin.z;
				}

				var boundsMax = render.bounds.max;
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