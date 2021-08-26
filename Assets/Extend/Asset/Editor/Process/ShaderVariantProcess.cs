using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Extend.Asset.Editor;
using Extend.Asset.Editor.Process;
using Extend.Common.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Extend.Asset.Editor.Process {
	public class ShaderVariantProcess : IBuildAssetProcess {
		public Type ProcessType => typeof(AssetImporter);

		private static readonly string[] SpecialShaders = {"Assets/Shaders/Effect/Fx_2TextureBlend.shader"};
		private readonly Dictionary<Shader, Tuple<string[], string[]>> m_shaderToGlobalKeywords = new Dictionary<Shader, Tuple<string[], string[]>>();
		private readonly ShaderVariantCollection m_collection = new ShaderVariantCollection();
		private const string m_collectionPath = "Assets/Shaders/Special.shadervariants";

		public ShaderVariantProcess() {
			AssetDatabase.DeleteAsset(m_collectionPath);
			foreach( var shaderPath in SpecialShaders ) {
				var shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
				var globalKeywords = ShaderUtilExtend.GetGlobalShaderKeywords(shader);
				var localKeywords = ShaderUtilExtend.GetLocalShaderKeywords(shader);
				
				m_shaderToGlobalKeywords.Add(shader, new Tuple<string[], string[]>(globalKeywords, localKeywords));
			}
		}

		public void Process(AssetImporter importer, TextWriter writer) {
			if( Path.GetExtension(importer.assetPath) != ".mat" ) {
				return;
			}

			var material = AssetDatabase.LoadAssetAtPath<Material>(importer.assetPath);
			if( !material ) {
				Debug.LogError($"Load material fail : {importer.assetPath}");
				return;
			}
			var shader = material.shader;
			var shaderPath = AssetDatabase.GetAssetPath(shader);
			if( shaderPath.StartsWith("Packages") ) {
				writer.WriteLine($"Material : {importer.assetPath} using in packages shader : {shaderPath}");
			}

			var materialKeywords = material.shaderKeywords;
			if( ShaderPreprocessor.AddInUsedShaderKeywords(shader, materialKeywords) &&
			    m_shaderToGlobalKeywords.TryGetValue(shader, out var localGlobal) ) {
				List<string> filteredKeywords = new List<string>(materialKeywords);
				for( int i = filteredKeywords.Count - 1; i >= 0; i-- ) {
					if( Array.IndexOf(localGlobal.Item2, filteredKeywords[i]) == -1 ) {
						filteredKeywords.RemoveAt(i);
					}
				}
				
				foreach( var keyword in localGlobal.Item1 ) {
					string[] keywords = new string[filteredKeywords.Count + 1];
					filteredKeywords.CopyTo(0, keywords, 0, filteredKeywords.Count);
					keywords[filteredKeywords.Count] = keyword;
					m_collection.Add(new ShaderVariantCollection.ShaderVariant(shader, PassType.ScriptableRenderPipeline, keywords));
				}
				m_collection.Add(new ShaderVariantCollection.ShaderVariant(shader, PassType.ScriptableRenderPipeline, filteredKeywords.ToArray()));
			}
		}

		public void PostProcess() {
			AssetDatabase.CreateAsset(m_collection, m_collectionPath);
			AssetDatabase.Refresh();
			AssetNode.GetOrCreate(m_collectionPath, "assets/shaders");
		}

		public void Clear() {
			ShaderPreprocessor.Clear();
		}

		[MenuItem("Tools/Asset/Show Importer")]
		private static void ShowImporter() {
			if( !Selection.activeObject )
				return;

			var path = AssetDatabase.GetAssetPath(Selection.activeObject);
			var importer = AssetImporter.GetAtPath(path);
			if( !importer ) {
				Debug.Log("Import is missing!");
				return;
			}

			Debug.Log($"Importer : {importer.GetType().FullName}");
		}
		
		[MenuItem("Tools/Asset/Show GUID")]
		private static void ShowGUID() {
			if( !Selection.activeObject )
				return;

			Debug.Log(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Selection.activeObject)));
		}
	}
}