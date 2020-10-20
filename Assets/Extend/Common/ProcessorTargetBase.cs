using UnityEngine;

namespace Extend.Common {
	public abstract class ProcessorTargetBase : MonoBehaviour {
		public abstract void ApplyProgress(float value);
	}
}