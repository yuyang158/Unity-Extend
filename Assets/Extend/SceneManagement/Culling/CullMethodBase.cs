using UnityEngine;

namespace Extend.SceneManagement.Culling {
	[DisallowMultipleComponent]
	public abstract class CullMethodBase : MonoBehaviour {
		public abstract Vector3 BoundsCenter { get; }

		public abstract bool Cull(Bounds bounds);
	}
}