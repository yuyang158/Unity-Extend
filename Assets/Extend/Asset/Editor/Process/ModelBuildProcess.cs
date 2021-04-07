using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Extend.Asset.Editor.Process {
	public class ModelProcess : IBuildAssetProcess {
		public Type ProcessType => typeof(ModelImporter);
		
		public void Process(AssetImporter importer, TextWriter writer) {
			var modelImporter = importer as ModelImporter;
			if(modelImporter.isReadable)
				writer.WriteLine($"WARNING\t{importer.assetPath} is readable");
			
			if(modelImporter.importCameras)
				writer.WriteLine($"WARNING\t{importer.assetPath} import camera");

			if( !modelImporter.optimizeGameObjects ) {
				var go = AssetDatabase.LoadAssetAtPath<GameObject>(importer.assetPath);
				var renderers = go.GetComponentsInChildren<SkinnedMeshRenderer>();
				if( renderers.Length > 0 ) {
					writer.WriteLine($"WARNING\t{importer.assetPath} need toggle optimize game object");
				}
			}
		}

		public void PostProcess() {
		}

		public void Clear() {
			
		}
	}
}