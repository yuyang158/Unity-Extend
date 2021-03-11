using System.IO;
using Extend.Common;
using Extend.LuaUtil;

using UnityEngine;
using XLua;

namespace Extend.Editor {
	[UnityEditor.AssetImporters.ScriptedImporter(1, "tsv")]
	public class TsvFileImporter : UnityEditor.AssetImporters.ScriptedImporter {
		public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx) {
			var text = File.ReadAllText(ctx.assetPath);
			var asset = new TextAsset(text);

			ctx.AddObjectToAsset("main obj", asset);
			ctx.SetMainObject(asset);
			
			if(!Application.isPlaying)
				return;

			var luaVm = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			var get = luaVm.Global.GetInPath<GetLuaService>( "_ServiceManager.GetService" );
			var configService = get(1);
			var reloadFunc = configService.Get<LuaFunction>("Reload");
			reloadFunc.Call(Path.GetFileNameWithoutExtension(ctx.assetPath));
		}
	}
}