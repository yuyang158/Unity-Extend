using UnityEngine;

namespace Extend.Common {
	public abstract class ProgressorTargetBase : MonoBehaviour {
		public abstract void ApplyProgress(float value);
	}
}