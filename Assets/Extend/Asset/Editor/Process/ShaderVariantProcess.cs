using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Extend.Asset.Editor.Process {
	public class ShaderVariantProcess : IBuildAssetProcess {
		public Type ProcessType => typeof(AssetImporter);

		public ShaderVariantProcess() {
			ShaderPreprocessor.Clear();
		}

		public void Process(AssetImporter importer, TextWriter writer) {
			if( Path.GetExtension(importer.assetPath) != ".mat" ) {
				return;
			}

			var material = AssetDatabase.LoadAssetAtPath<Material>(importer.assetPath);
			var shaderPath = AssetDatabase.GetAssetPath(material.shader);
			if( shaderPath.StartsWith("Packages") ) {
				writer.WriteLine($"Material : {importer.assetPath} using in packages shader : {shaderPath}");
			}

			ShaderPreprocessor.AddInUsedShaderKeywords(material.shader, material.shaderKeywords);
		}

		public void PostProcess() {
		}

		[MenuItem("Tools/Editor/Show Importer")]
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

		[MenuItem("Tools/Editor/Show Keywords")]
		private static void ShowKeywords() {
			if( !( Selection.activeObject is Material ) )
				return;

			var mat = Selection.activeObject as Material;
			Debug.Log(string.Join(";", mat.shaderKeywords));
		}
	}
}