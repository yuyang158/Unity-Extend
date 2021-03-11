using Extend.Common;
using Extend.Render;
using UnityEngine;

namespace Extend.UI.Fx {
	public class UIAreaBlurMark : MonoBehaviour {
		private Camera m_cachedCamera;

		private void OnEnable() {
			var canvas = GetComponentInParent<Canvas>();
			m_cachedCamera = canvas.worldCamera;
			RenderFeatureService.Get().SetFeatureActive(m_cachedCamera, "BoxBlur", true);
		}

		private void OnDisable() {
			if( !CSharpServiceManager.Initialized )
				return;
			RenderFeatureService.Get().SetFeatureActive(m_cachedCamera, "BoxBlur", false);
		}
	}
}