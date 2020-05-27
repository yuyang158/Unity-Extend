using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Extend.Common;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using XLua;
using Debug = UnityEngine.Debug;

namespace Extend.Editor {
	[ScriptedImporter(1, "lua")]
	public class LuaScriptImporter : ScriptedImporter {
		[MenuItem("XLua/hotfix")]
		private static void Hotfix() {
			if( !Application.isPlaying )
				return;

			var luaVm = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			var hotfixModule = luaVm.LoadFileAtPath("hotfix.hotfix")[0] as LuaTable;
			var func = hotfixModule.Get<Action<string>>("hotfix_module");
			foreach( var module in modifiedModules ) {
				func(module);
			}

			modifiedModules.Clear();
		}

		private static readonly List<string> modifiedModules = new List<string>();

		static LuaScriptImporter() {
			Application.quitting += () => { modifiedModules.Clear(); };
		}

		public override void OnImportAsset(AssetImportContext ctx) {
			if( Application.isPlaying ) {
				var path = ctx.assetPath.Substring(21, ctx.assetPath.Length - 21 - 4);
				var moduleName = path.Replace('/', '.');
				modifiedModules.Add(moduleName);
			}

			var proc = new Process {
				StartInfo = new ProcessStartInfo {
					FileName = @"E:\tools\luacheck.exe",
					Arguments = ctx.assetPath,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					CreateNoWindow = true
				}
			};
			proc.Start();
			var builder = new StringBuilder(1024);
			while( !proc.StandardOutput.EndOfStream ) {
				var line = proc.StandardOutput.ReadLine();
				builder.AppendLine(line);
			}
			Debug.Log(builder.ToString());

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