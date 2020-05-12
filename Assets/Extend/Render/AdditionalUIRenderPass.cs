using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Extend.Render {
	public class AdditionalUIRenderPass : ScriptableRenderPass {
		private readonly RenderQueueType renderQueueType;
		private FilteringSettings m_FilteringSettings;
		private readonly string m_ProfilerTag;
		private readonly ProfilingSampler m_ProfilingSampler;

		private readonly List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();

		public void SetDepthState(bool writeEnabled, CompareFunction function = CompareFunction.Less) {
			m_RenderStateBlock.mask |= RenderStateMask.Depth;
			m_RenderStateBlock.depthState = new DepthState(writeEnabled, function);
		}

		public void SetStencilState(int reference, CompareFunction compareFunction, StencilOp passOp, StencilOp failOp, StencilOp zFailOp) {
			var stencilState = StencilState.defaultValue;
			stencilState.enabled = true;
			stencilState.SetCompareFunction(compareFunction);
			stencilState.SetPassOperation(passOp);
			stencilState.SetFailOperation(failOp);
			stencilState.SetZFailOperation(zFailOp);

			m_RenderStateBlock.mask |= RenderStateMask.Stencil;
			m_RenderStateBlock.stencilReference = reference;
			m_RenderStateBlock.stencilState = stencilState;
		}

		private RenderStateBlock m_RenderStateBlock;

		public AdditionalUIRenderPass(string profilerTag, RenderPassEvent renderPassEvent, string[] shaderTags, RenderQueueType renderQueueType, int layerMask) {
			m_ProfilerTag = profilerTag;
			m_ProfilingSampler = new ProfilingSampler(profilerTag);
			this.renderPassEvent = renderPassEvent;
			this.renderQueueType = renderQueueType;
			var renderQueueRange = renderQueueType == RenderQueueType.Transparent
				? RenderQueueRange.transparent
				: RenderQueueRange.opaque;
			m_FilteringSettings = new FilteringSettings(renderQueueRange, layerMask);

			if( shaderTags != null && shaderTags.Length > 0 ) {
				foreach( var passName in shaderTags )
					m_ShaderTagIdList.Add(new ShaderTagId(passName));
			}
			else {
				m_ShaderTagIdList.Add(new ShaderTagId("UniversalForward"));
				m_ShaderTagIdList.Add(new ShaderTagId("LightweightForward"));
				m_ShaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
			}

			m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
			var sortingCriteria = renderQueueType == RenderQueueType.Transparent
				? SortingCriteria.CommonTransparent
				: renderingData.cameraData.defaultOpaqueSortFlags;

			var drawingSettings = CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, sortingCriteria);
			var cmd = CommandBufferPool.Get(m_ProfilerTag);
			using( new ProfilingScope(cmd, m_ProfilingSampler) ) {
				context.ExecuteCommandBuffer(cmd);
				cmd.Clear();
				cmd.ClearRenderTarget((clearFlag & ClearFlag.Depth) != 0, false, Color.black);
				context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref m_FilteringSettings,
					ref m_RenderStateBlock);
			}

			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}
	}
}