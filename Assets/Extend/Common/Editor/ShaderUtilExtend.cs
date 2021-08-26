using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Extend.Common.Editor {
	public static class ShaderUtilExtend {
		public static string[] GetGlobalShaderKeywords(Shader shader) {
			var type = typeof(ShaderUtil);
			var getShaderGlobalKeywords = type.GetMethod("GetShaderGlobalKeywords", 
				BindingFlags.Static | BindingFlags.NonPublic);
			return getShaderGlobalKeywords.Invoke(null, new object[] {shader}) as string[];
		}
		
		public static string[] GetLocalShaderKeywords(Shader shader) {
			var type = typeof(ShaderUtil);
			var getShaderLocalKeywords = type.GetMethod("GetShaderLocalKeywords", 
				BindingFlags.Static | BindingFlags.NonPublic);
			return getShaderLocalKeywords.Invoke(null, new object[] {shader}) as string[];
		}
	}
}