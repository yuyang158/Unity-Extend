using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Extend.Render {
	public class AdditionalRender : ScriptableRendererFeature {
		[System.Serializable]
		public class RenderObjectsSettings {
			public string passTag = "AdditionalRender";
			public RenderPassEvent Event = RenderPassEvent.AfterRenderingPostProcessing;

			public FilterSettings filterSettings = new FilterSettings();

			public Material overrideMaterial = null;
			public int overrideMaterialPassIndex = 0;

			public bool overrideDepthState = false;
			public CompareFunction depthCompareFunction = CompareFunction.LessEqual;
			public bool enableWrite = true;

			public StencilStateData stencilSettings = new StencilStateData();

			public RenderObjects.CustomCameraSettings cameraSettings = new RenderObjects.CustomCameraSettings();
		}

		[System.Serializable]
		public class FilterSettings {
			// TODO: expose opaque, transparent, all ranges as drop down
			public RenderQueueType RenderQueueType;
			public LayerMask LayerMask;
			public string[] PassNames;

			public FilterSettings() {
				RenderQueueType = RenderQueueType.Opaque;
				LayerMask = 0;
			}
		}

		public RenderObjectsSettings settings = new RenderObjectsSettings();

		RenderObjectsPass renderObjectsPass;

		public override void Create() {
			var filter = settings.filterSettings;
			renderObjectsPass = new RenderObjectsPass(settings.passTag, settings.Event, filter.PassNames,
				filter.RenderQueueType, filter.LayerMask, settings.cameraSettings) {
				overrideMaterial = settings.overrideMaterial,
				overrideMaterialPassIndex = settings.overrideMaterialPassIndex
			};


			if( settings.overrideDepthState )
				renderObjectsPass.SetDetphState(settings.enableWrite, settings.depthCompareFunction);

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