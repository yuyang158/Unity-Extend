using System.IO;
using UnityEngine;


namespace Extend.Editor {
	[UnityEditor.AssetImporters.ScriptedImporter(1, "ini")]
	public class IniImporter : UnityEditor.AssetImporters.ScriptedImporter {
		public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx) {
			var text = File.ReadAllText(ctx.assetPath);
			var asset = new TextAsset(text);
			ctx.AddObjectToAsset("main obj", asset);
			ctx.SetMainObject(asset);
		}
	}
}