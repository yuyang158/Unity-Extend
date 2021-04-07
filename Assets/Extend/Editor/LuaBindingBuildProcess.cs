using System;
using System.IO;
using Extend.Asset.Editor.Process;
using Extend.Common.Editor;
using Extend.LuaBindingData;
using UnityEditor;
using UnityEngine;

namespace Extend.Editor {
	public class LuaBindingBuildProcess : IBuildAssetProcess {
		public Type ProcessType => typeof(AssetImporter);

		public void Process(AssetImporter importer, TextWriter writer) {
			if( Path.GetExtension(importer.assetPath) != ".prefab" )
				return;

			var go = AssetDatabase.LoadAssetAtPath<GameObject>(importer.assetPath);
			var bindings = go.GetComponentsInParent<LuaBinding>();

			foreach( var binding in bindings ) {
				foreach( var data in binding.LuaData ) {
					if( !( data is LuaBindingAssetReferenceData refData ) || refData.Data.GUIDValid ) {
						continue;
					}

					writer.WriteLine($"ERROR\t{importer.assetPath}:{UIEditorUtil.RecursiveNodePath(binding.transform)} Lua Binding Asset Reference is missing");
				}
			}
		}

		public void PostProcess() {
		}

		public void Clear() {
		}
	}
}