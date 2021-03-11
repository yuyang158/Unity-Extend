using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using XLua;

namespace Extend.Render {
	[LuaCallCSharp]
	public class BoxBlurRenderer : PostProcessRenderer {
		[Serializable, LuaCallCSharp]
		public class AdditionalRenderSettings : Settings {
			public float RTDownScaling = 2;
			public int Iteration = 6;
			public float BlurRadius = 1;

			public AdditionalRenderSettings() {
				PassTag = "BoxBlurRenderer";
			}
		}

		[SerializeField]
		private AdditionalRenderSettings m_settings = new AdditionalRenderSettings();
		private BoxBlurRenderPass m_blurPass;

		
		public override void Create() {
			m_blurPass = new BoxBlurRenderPass(m_settings);
		}

		protected override ScriptableRenderPass RenderPass => m_blurPass;
		protected override Settings RendererSettings => m_settings;
		protected override void PreparePass(ScriptableRenderer renderer, ref RenderingData renderingData) {
		}

		private void OnDestroy() {
			m_blurPass.Destroy();
		}
	}
}