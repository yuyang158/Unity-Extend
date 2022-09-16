using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Extend.Render {
	public class AdditionalUIRender : ScriptableRendererFeature {
		[System.Serializable]
		public class AdditionalRenderSettings {
			public string passTag = "AdditionalUIRender";
			public RenderPassEvent Event = RenderPassEvent.AfterRenderingPostProcessing;

			public FilterSettings filterSettings = new FilterSettings();

			public bool overrideDepthState;
			public CompareFunction depthCompareFunction = CompareFunction.LessEqual;
			public bool enableWrite = true;

			public StencilStateData stencilSettings = new StencilStateData();
			public ClearFlag clearFlag;
		}

		[System.Serializable]
		public class FilterSettings {
			// TODO: expose opaque, transparent, all ranges as drop down
			public LayerMask LayerMask;
			public string[] PassNames;

			public FilterSettings() {
				LayerMask = 0;
			}
		}

		public AdditionalRenderSettings settings = new AdditionalRenderSettings();
		private AdditionalUIRenderPass renderObjectsPass;

		public override void Create() {
			var filter = settings.filterSettings;
			renderObjectsPass = new AdditionalUIRenderPass(settings.passTag, settings.Event, filter.PassNames,
				filter.LayerMask, settings.clearFlag);

			if( settings.overrideDepthState )
				renderObjectsPass.SetDepthState(settings.enableWrite, settings.depthCompareFunction);

			if( settings.stencilSettings.overrideStencilState )
				renderObjectsPass.SetStencilState(settings.stencilSettings.stencilReference,
					settings.stencilSettings.stencilCompareFunction, settings.stencilSettings.passOperation,
					settings.stencilSettings.failOperation, settings.stencilSettings.zFailOperation);
		}

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
			renderer.EnqueuePass(renderObjectsPass);
		}
	}
}