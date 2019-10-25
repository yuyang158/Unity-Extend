using System.IO;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace Extend.Editor {
	[ScriptedImporter(1, "lua")]
	public class LuaScriptImporter : ScriptedImporter {
		public override void OnImportAsset(AssetImportContext ctx) {
			var text = File.ReadAllText(ctx.assetPath);
			var asset = new TextAsset(text);
			ctx.AddObjectToAsset("main obj", asset);
			ctx.SetMainObject(asset);
		}
	}
}