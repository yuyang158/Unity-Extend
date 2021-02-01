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
			ShaderPreprocessor.AddInUsedShaderKeywords(material.shader, material.shaderKeywords);
		}

		public void PostProcess() {
		}

		private static void ShowImporter() {
			if( !Selection.activeObject )
				return;

			var path = AssetDatabase.GetAssetPath(Selection.activeObject);
			var importer = AssetImporter.GetAtPath(path);
			if(!importer)
				return;
			Debug.Log($"Importer : {importer.GetType().FullName}");
		}
	}
}