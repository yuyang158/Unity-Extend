using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Extend.Common;
using Extend.Common.Editor;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using XLua;
using Debug = UnityEngine.Debug;

namespace Extend.Editor {
	[ScriptedImporter(1, "lua"), InitializeOnLoad]
	public class LuaScriptImporter : ScriptedImporter {
		[MenuItem("XLua/hotfix")]
		private static void Hotfix() {
			if( !Application.isPlaying )
				return;

			var luaVm = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			var hotfixModule = luaVm.LoadFileAtPath("hotfix.hotfix")[0] as LuaTable;
			var func = hotfixModule.Get<Action<string>>("hotfix_module");
			lock( modifiedModules ) {
				foreach( var module in modifiedModules ) {
					func(module);
				}

				modifiedModules.Clear();
			}
		}

		private static readonly List<string> modifiedModules = new();
		private static bool m_playing;

		static LuaScriptImporter() {
			EditorApplication.playModeStateChanged += change => {
				if( change == PlayModeStateChange.EnteredPlayMode ) {
					modifiedModules.Clear();
					m_playing = true;
				}
				else if( change == PlayModeStateChange.EnteredEditMode ) {
					m_playing = false;
				}
			};
			Application.quitting += () => { modifiedModules.Clear(); };

			var directories = Directory.GetDirectories($"{Environment.CurrentDirectory}\\Lua", "*", SearchOption.AllDirectories);
			foreach( var directory in directories ) {
				var watcher = new FileSystemWatcher(directory, "*.lua");
				watcher.Changed += LuaFileChangedCallback;
				watcher.EnableRaisingEvents = true;
			}
			var rootWatcher = new FileSystemWatcher($"{Environment.CurrentDirectory}\\Lua", "*.lua");
			rootWatcher.Changed += LuaFileChangedCallback;
			rootWatcher.EnableRaisingEvents = true;
		}

		private static void LuaFileChangedCallback(object _, FileSystemEventArgs args) {
			EditorMainThreadDispatcher.PushAction(() => {
				ExecLuaCheck(args.FullPath);
			});
			if( !m_playing )
				return;
			var path = args.FullPath;
			var start = path.IndexOf("Lua", StringComparison.CurrentCulture) + 4;
			var module = path.Substring(start, path.Length - start - 4).Replace('\\', '.');
			lock( modifiedModules ) {
				modifiedModules.Add(module);
			}
		}

		public override void OnImportAsset(AssetImportContext ctx) {
			if( Application.isPlaying ) {
				var path = ctx.assetPath.Substring(21, ctx.assetPath.Length - 21 - 4);
				var moduleName = path.Replace('/', '.');
				lock( modifiedModules ) {
					modifiedModules.Add(moduleName);
				}
			}

			ExecLuaCheck(ctx.assetPath);
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

		private static void ExecLuaCheck(string luaPath) {
			if( !File.Exists(luaPath) ) {
				return;
			}
			
			luaPath = luaPath.Replace('\\', '/');
			var index = luaPath.IndexOf("Lua/", StringComparison.InvariantCulture);
			var projectPath = luaPath.Substring(index + 4, luaPath.Length - index - 4 - 4);
			LuaClassEditorFactory.ReloadDescriptor(projectPath);
			var setting = LuaCheckSetting.GetOrCreateSettings();
			if( setting.Active && !string.IsNullOrEmpty(setting.LuaCheckExecPath) && File.Exists(setting.LuaCheckExecPath) ) {
				EditorApplication.delayCall += () => {
					// https://github.com/lunarmodules/luacheck
					var proc = new Process {
						StartInfo = new ProcessStartInfo {
							FileName = setting.LuaCheckExecPath,
							Arguments = $"{luaPath} --no-global --max-line-length {setting.MaxLineLength} --std lua54 --no-self",
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
				};
			}
		}
	}
}