using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Extend.Render {
	public class AdditionalUIRenderPass : ScriptableRenderPass {
		private readonly RenderQueueType m_renderQueueType;
		private FilteringSettings m_filteringSettings;
		private readonly string m_profilerTag;
		private readonly ProfilingSampler m_profilingSampler;

		private readonly List<ShaderTagId> m_shaderTagIdList = new List<ShaderTagId>();

		public void SetDepthState(bool writeEnabled, CompareFunction function = CompareFunction.Less) {
			m_renderStateBlock.mask |= RenderStateMask.Depth;
			m_renderStateBlock.depthState = new DepthState(writeEnabled, function);
		}

		public void SetStencilState(int reference, CompareFunction compareFunction, StencilOp passOp, StencilOp failOp, StencilOp zFailOp) {
			var stencilState = StencilState.defaultValue;
			stencilState.enabled = true;
			stencilState.SetCompareFunction(compareFunction);
			stencilState.SetPassOperation(passOp);
			stencilState.SetFailOperation(failOp);
			stencilState.SetZFailOperation(zFailOp);

			m_renderStateBlock.mask |= RenderStateMask.Stencil;
			m_renderStateBlock.stencilReference = reference;
			m_renderStateBlock.stencilState = stencilState;
		}

		private RenderStateBlock m_renderStateBlock;
		private ClearFlag m_clearFlag;

		public AdditionalUIRenderPass(string profilerTag, RenderPassEvent renderPassEvent, string[] shaderTags, RenderQueueType renderQueueType, int layerMask, ClearFlag clearFlag) {
			m_profilerTag = profilerTag;
			m_profilingSampler = new ProfilingSampler(profilerTag);
			this.renderPassEvent = renderPassEvent;
			m_renderQueueType = renderQueueType;
			m_clearFlag = clearFlag;
			var renderQueueRange = renderQueueType == RenderQueueType.Transparent
				? RenderQueueRange.transparent
				: RenderQueueRange.opaque;
			m_filteringSettings = new FilteringSettings(renderQueueRange, layerMask);

			if( shaderTags != null && shaderTags.Length > 0 ) {
				foreach( var passName in shaderTags )
					m_shaderTagIdList.Add(new ShaderTagId(passName));
			}
			else {
				m_shaderTagIdList.Add(new ShaderTagId("UniversalForward"));
				m_shaderTagIdList.Add(new ShaderTagId("LightweightForward"));
				m_shaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
			}

			m_renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
			var sortingCriteria = m_renderQueueType == RenderQueueType.Transparent
				? SortingCriteria.CommonTransparent
				: renderingData.cameraData.defaultOpaqueSortFlags;

			var drawingSettings = CreateDrawingSettings(m_shaderTagIdList, ref renderingData, sortingCriteria);
			var cmd = CommandBufferPool.Get(m_profilerTag);
			using( new ProfilingScope(cmd, m_profilingSampler) ) {
				context.ExecuteCommandBuffer(cmd);
				cmd.Clear();
				cmd.ClearRenderTarget((m_clearFlag & ClearFlag.Depth) != 0, false, Color.black);
				context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref m_filteringSettings,
					ref m_renderStateBlock);
			}

			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}
	}
}