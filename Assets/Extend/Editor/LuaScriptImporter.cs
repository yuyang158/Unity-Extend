using System.IO;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace Extend.Editor {
	[ScriptedImporter(1, "lua")]
	public class LuaScriptImporter : ScriptedImporter {
		public override void OnImportAsset(AssetImportContext ctx) {
			var text = File.ReadAllText(ctx.assetPath);
			var asset = new TextAsset(text);
			using( var reader = new StringReader(asset.text) ) {
				var line = reader.ReadLine();
				while( line != null ) {
					if( line.StartsWith("---@class") ) {
						var statements = line.Split(' ');
						var className = statements[1];
						LuaClassEditorFactory.ReloadDescriptor(className);
						break;
					}
					
					line = reader.ReadLine();
				}
			}
			ctx.AddObjectToAsset("main obj", asset);
			ctx.SetMainObject(asset);
		}
	}
}