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
			new ShaderKeyword("FOG_EXP"),
			new ShaderKeyword("FOG_EXP2"),
			new ShaderKeyword("POINT_COOKIE"),
			new ShaderKeyword("DIRECTIONAL_COOKIE"),
			new ShaderKeyword("SHADOWS_SOFT"),
			new ShaderKeyword("VERTEXLIGHT_ON"),
			new ShaderKeyword("DYNAMICLIGHTMAP_ON"),
			new ShaderKeyword("LOD_FADE_CROSSFADE")
		};

		private static readonly Dictionary<Shader, List<string[]>> m_shaderKeywordCollector = new Dictionary<Shader, List<string[]>>();

		public static void Clear() {
			m_shaderKeywordCollector.Clear();
		}

		public static void AddInUsedShaderKeywords(Shader shader, string[] keywords) {
			if( !m_shaderKeywordCollector.TryGetValue(shader, out var keywordLists) ) {
				keywordLists = new List<string[]>();
				m_shaderKeywordCollector.Add(shader, keywordLists);
			}

			keywordLists.Add(keywords);
		}

		public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data) {
			if( !m_shaderKeywordCollector.TryGetValue(shader, out var collectKeywords) ) {
				data.Clear();
				return;
			}

			for( int i = data.Count - 1; i >= 0; i-- ) {
				var compilerData = data[i];
				var keywords = compilerData.shaderKeywordSet.GetShaderKeywords();
				foreach( var shaderKeyword in keywords ) {
					if( Array.IndexOf(STRIP_BUILD_IN_KEYWORDS, shaderKeyword) != -1 ) {
						data.RemoveAt(i);
						break;
					}

					var keywordName = ShaderKeyword.GetKeywordName(shader, shaderKeyword);
					bool inUsed = false;
					foreach( var collectKeyword in collectKeywords ) {
						if( collectKeyword.Contains(keywordName) ) {
							inUsed = true;
							break;
						}
					}

					if( !inUsed ) {
						data.RemoveAt(i);
						break;
					}
				}
			}

			Debug.Log($"Shader {shader.name} --> {data.Count}");
		}
	}
}