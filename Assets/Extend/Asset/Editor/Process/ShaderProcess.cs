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
			new ShaderKeyword("VERTEXLIGHT_ON"),
			new ShaderKeyword("DYNAMICLIGHTMAP_ON"),
			new ShaderKeyword("LOD_FADE_CROSSFADE")
		};

		private static readonly string[] URP_SKIP_KEYWORDS = {
			"_MAIN_LIGHT_SHADOWS",
			"_MAIN_LIGHT_SHADOWS_CASCADE",
			// "_ADDITIONAL_LIGHTS",
			// "_ADDITIONAL_LIGHTS_VERTEX",
			// "_SCREEN_SPACE_OCCLUSION",
			// "_ADDITIONAL_LIGHT_SHADOWS",
			"_SHADOWS_SOFT"
		};

		private static readonly Dictionary<Shader, List<string[]>> m_shaderKeywordCollector = new Dictionary<Shader, List<string[]>>();
		private static readonly List<string> m_filteredBuildInKeywords = new List<string>();

		public static void Clear() {
			m_shaderKeywordCollector.Clear();

			Debug.LogWarning("Builtin keywords : " + string.Join(";", m_filteredBuildInKeywords));
			m_filteredBuildInKeywords.Clear();
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

		public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data) {
			Debug.Log($"Before Shader {shader.name} --> {data.Count}");
			if( !m_shaderKeywordCollector.TryGetValue(shader, out var collectKeywords) ) {
				if( m_shaderKeywordCollector.Count == 0 ) {
					return;
				}

				data.Clear();
				Debug.Log($"Shader {shader.name} --> {data.Count}");
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
					if( !keywordName.StartsWith("_") ) {
						if( !m_filteredBuildInKeywords.Contains(keywordName) )
							m_filteredBuildInKeywords.Add(keywordName);
						continue;
					}
					
					if(URP_SKIP_KEYWORDS.Contains(keywordName))
						continue;

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