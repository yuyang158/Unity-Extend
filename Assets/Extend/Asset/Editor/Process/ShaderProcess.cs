using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace Extend.Asset.Editor.Process {
	internal class ShaderPreprocessor : IPreprocessShaders {
		public int callbackOrder => 0;

		public static readonly ShaderKeyword[] STRIP_BUILD_IN_KEYWORDS = {
			new ShaderKeyword("_ADDITIONAL_LIGHTS_VERTEX"),
			new ShaderKeyword("_DBUFFER_MRT1"),
			new ShaderKeyword("_DBUFFER_MRT2"),
			new ShaderKeyword("_DBUFFER_MRT3"),
			new ShaderKeyword("_MAIN_LIGHT_SHADOWS_CASCADE"),
			new ShaderKeyword("_SHADOWS_SOFT")
		};

		private static readonly string[] URP_SKIP_KEYWORDS = {
			"INSTANCING_ON",
			"_MAIN_LIGHT_SHADOWS",
			// "_MAIN_LIGHT_SHADOWS_CASCADE",
			"_MAIN_LIGHT_SHADOWS_SCREEN",
			"_ADDITIONAL_LIGHTS",
			// "_ADDITIONAL_LIGHTS_VERTEX",
			"_SCREEN_SPACE_OCCLUSION",
			// "_ADDITIONAL_LIGHT_SHADOWS",
			"_REFLECTION_PROBE_BLENDING",
			"_REFLECTION_PROBE_BOX_PROJECTION",
			// "_SHADOWS_SOFT",
			"_EMISSION",
			"_MIXED_LIGHTING_SUBTRACTIVE",
			"_LIGHT_LAYERS",
			"_LIGHT_COOKIES",
			// "_DBUFFER_MRT1",
			// "_DBUFFER_MRT2",
			// "_DBUFFER_MRT3",

			//GBUFFER
			// "_GBUFFER_NORMALS_OCT",
			"_RENDER_PASS_ENABLED",
			"_CASTING_PUNCTUAL_LIGHT_SHADOW",
			"PROCEDURAL_INSTANCING_ON",

			// FOG
			"FOG_EXP2"
		};

		private static readonly Dictionary<Shader, List<string[]>> m_shaderKeywordCollector = new Dictionary<Shader, List<string[]>>();

		public static void Clear() {
			m_shaderKeywordCollector.Clear();
		}

		public static bool AddInUsedShaderKeywords(Shader shader, string[] keywords) {
			if( !m_shaderKeywordCollector.TryGetValue(shader, out var keywordLists) ) {
				keywordLists = new List<string[]>();
				m_shaderKeywordCollector.Add(shader, keywordLists);
			}

			Array.Sort(keywords);
			bool newKeyword = false;
			if( keywordLists.FindIndex(existList => {
				if( keywords.Length != existList.Length )
					return false;

				return !keywords.Where((t, i) => t != existList[i]).Any();
			}) == -1 ) {
				keywordLists.Add(keywords);
				newKeyword = true;
			}

			return newKeyword;
		}

		private static readonly string[] m_ignoreShaderNames = {"VolumetricFog", "Hidden"};
		public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data) {
			// Debug.Log($"Before Shader {shader.name} --> {data.Count}");
			if( data.Count == 1 ) {
				return;
			}

			if( m_ignoreShaderNames.Any(shaderName => shader.name.IndexOf(shaderName, StringComparison.Ordinal) != -1) ) {
				return;
			}
			
			if( !m_shaderKeywordCollector.TryGetValue(shader, out var collectKeywords) ) {
				if( m_shaderKeywordCollector.Count == 0 ) {
					return;
				}

				data.Clear();
				// Debug.Log($"Shader {shader.name} --> {data.Count}");
				return;
			}

			for( int i = data.Count - 1; i >= 0; i-- ) {
				var compilerData = data[i];
				var keywords = compilerData.shaderKeywordSet.GetShaderKeywords();
				if( keywords.Any(shaderKeyword => Array.IndexOf(STRIP_BUILD_IN_KEYWORDS, shaderKeyword) != -1) ) {
					data.RemoveAt(i);
				}
			}

			// Debug.Log($"Shader {shader.name} --> {data.Count}");
		}
	}
}