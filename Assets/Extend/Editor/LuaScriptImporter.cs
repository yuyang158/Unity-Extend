using System.Collections.Generic;
using System.IO;
using Extend.Common;
using Extend.Common.Lua;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using XLua;

namespace Extend.Editor {
	[ScriptedImporter(1, "lua")]
	public class LuaScriptImporter : ScriptedImporter {
		[MenuItem("XLua/hotfix")]
		private static void Hotfix() {
			if(!Application.isPlaying)
				return;

			var luaVm = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			var hotfixModule = luaVm.LoadFileAtPath("hotfix.hotfix")[0] as ILuaTable;
			var func = hotfixModule.Get<LuaFunction>("hotfix_module");
			foreach( var module in modifiedModules ) {
				func.Call(module);
			}
			modifiedModules.Clear();
		}
		private static readonly List<string> modifiedModules = new List<string>();

		static LuaScriptImporter() {
			Application.quitting += () => {
				modifiedModules.Clear();
			};
		}
		
		public override void OnImportAsset(AssetImportContext ctx) {
			if( Application.isPlaying ) {
				var path = ctx.assetPath.Substring(21, ctx.assetPath.Length - 21 - 4);
				var moduleName = path.Replace('/', '.');
				modifiedModules.Add(moduleName);
			}
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