using System.IO;
using Extend.Common;
using Extend.LuaUtil;
using System.Collections;
using Unity.EditorCoroutines.Editor;
using UnityEditor.AssetImporters;
using UnityEngine;
using XLua;

namespace Extend.Editor {
	[ScriptedImporter(1, "tsv")]
	public class TsvFileImporter : ScriptedImporter {
		public override void OnImportAsset(AssetImportContext ctx) {
			var text = File.ReadAllText(ctx.assetPath);
			var asset = new TextAsset(text);

			ctx.AddObjectToAsset("main obj", asset);
			ctx.SetMainObject(asset);
			
			if(!Application.isPlaying)
				return;
			EditorCoroutineUtility.StartCoroutineOwnerless(ReLoad(Path.GetFileNameWithoutExtension(ctx.assetPath)));
		}

		private static IEnumerator ReLoad(string path) {
			yield return new EditorWaitForSeconds(0.1f); 
			var luaVm = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			var get = luaVm.Global.GetInPath<GetLuaService>( "_ServiceManager.GetService" );
			var configService = get(1);
			var reloadFunc = configService.Get<LuaFunction>("Reload");
			reloadFunc.Call(path);
		}
	}
}