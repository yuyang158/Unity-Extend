using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

namespace Extend.Render {
	public class BoxBlurRenderPass : ScriptableRenderPass {
		private readonly BoxBlurRenderer.AdditionalRenderSettings m_settings;

		private static class ShaderIDs {
			internal static readonly int BlurRadius = Shader.PropertyToID("_BlurOffset");
			internal static readonly int BufferRT1 = Shader.PropertyToID("_BufferRT1");
			internal static readonly int BufferRT2 = Shader.PropertyToID("_BufferRT2");
		}

		private Material m_material;
		private readonly int m_destinationId;
		const string k_BoxBlurConstants = "Box Blur";
		private static readonly ProfilingSampler m_ProfilingSampler = new ProfilingSampler(k_BoxBlurConstants);

		public BoxBlurRenderPass(BoxBlurRenderer.AdditionalRenderSettings settings) {
			m_settings = settings;
			renderPassEvent = RenderPassEvent.AfterRendering;
			m_destinationId = Shader.PropertyToID("_BoxBlurTexture");
			profilingSampler = new ProfilingSampler(nameof(BoxBlurRenderPass));
		}

		private RenderTextureDescriptor m_cameraTextureDescriptor;

		public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
			m_cameraTextureDescriptor = cameraTextureDescriptor;
			m_cameraTextureDescriptor.depthBufferBits = 0;
			m_cameraTextureDescriptor.stencilFormat = GraphicsFormat.None;
		}

		private RenderTargetHandle m_source;

		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
			/*_source = renderingData.cameraData.renderer.afterPostProcessTarget;
			if( m_material != null )
				return;
			var shader = Shader.Find("Hidden/PostProcessing/BoxBlur");
			if( !shader )
				return;
			m_material = new Material(shader);*/
		}

		private static readonly int sourceTex = Shader.PropertyToID("_SourceTex");

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
			var camera = renderingData.cameraData.camera;
			//if( camera.targetTexture == null && !PostProcessPass.PostProcessPassSetuped )
				//return;
			if( !m_material )
				return;

			var cmd = CommandBufferPool.Get(m_settings.PassTag);
			using( new ProfilingScope(cmd, m_ProfilingSampler) ) {
				m_cameraTextureDescriptor.width = (int)( m_cameraTextureDescriptor.width / m_settings.RTDownScaling );
				m_cameraTextureDescriptor.height = (int)( m_cameraTextureDescriptor.height / m_settings.RTDownScaling );
				cmd.GetTemporaryRT(ShaderIDs.BufferRT1, m_cameraTextureDescriptor);
				cmd.GetTemporaryRT(ShaderIDs.BufferRT2, m_cameraTextureDescriptor);

				var target = camera.targetTexture;
				Blit(cmd, target ? target : m_source.Identifier(), ShaderIDs.BufferRT1);
				var BlurRadius = new Vector4(m_settings.BlurRadius / m_cameraTextureDescriptor.width, 
					m_settings.BlurRadius / m_cameraTextureDescriptor.height, 0, 0);
				m_material.SetVector(ShaderIDs.BlurRadius, BlurRadius);
				for( int i = 0; i < m_settings.Iteration; i++ ) {
					if( m_settings.Iteration > 20 ) {
						return;
					}

					cmd.SetGlobalTexture(sourceTex, ShaderIDs.BufferRT1);
					Blit(cmd, ShaderIDs.BufferRT1, ShaderIDs.BufferRT2, m_material);
					cmd.SetGlobalTexture(sourceTex, ShaderIDs.BufferRT2);
					Blit(cmd, ShaderIDs.BufferRT2, ShaderIDs.BufferRT1, m_material);
				}

				cmd.SetGlobalTexture(m_destinationId, ShaderIDs.BufferRT1);
				if( m_settings.BlitToSource ) {
					cmd.SetGlobalTexture(sourceTex, ShaderIDs.BufferRT1);
					Blit(cmd, ShaderIDs.BufferRT1, target ? target : m_source.Identifier(), m_material, 1);
				}
			}

			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}

		public override void OnFinishCameraStackRendering(CommandBuffer cmd) {
			cmd.ReleaseTemporaryRT(ShaderIDs.BufferRT1);
			cmd.ReleaseTemporaryRT(ShaderIDs.BufferRT2);
		}

		public void Destroy() {
			Object.Destroy(m_material);
		}
	}
}