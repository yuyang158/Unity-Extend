using UnityEngine;

namespace Extend.UI.Scroll {
	public interface ILoopScrollDataProvider {
		void ProvideData(Transform t, int index);
	}
}