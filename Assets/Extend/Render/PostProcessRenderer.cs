using System;
using UnityEngine.Rendering.Universal;

namespace Extend.Render {
	public abstract class PostProcessRenderer : ScriptableRendererFeature {
		[Serializable]
		public class Settings {
			public string PassTag;
			public bool ActiveInSceneView;
			public bool BlitToSource;
		}

		protected abstract ScriptableRenderPass RenderPass { get; }

		protected abstract Settings RendererSettings { get; }

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
			ref var cameraData = ref renderingData.cameraData;
			if( cameraData.isSceneViewCamera && !RendererSettings.ActiveInSceneView )
				return;

			if( !cameraData.postProcessEnabled && cameraData.targetTexture == null )
				return;

			PreparePass(renderer, ref renderingData);
			renderer.EnqueuePass(RenderPass);
		}

		protected abstract void PreparePass(ScriptableRenderer renderer, ref RenderingData renderingData);
	}
}