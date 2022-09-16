using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Extend.Common;
using Extend.Common.Editor;
using UnityEditor;
using UnityEngine;
using XLua;
using Debug = UnityEngine.Debug;

namespace Extend.Editor {
	[InitializeOnLoad]
	public class LuaScriptImporter {
		[MenuItem("XLua/hotfix")]
		public static void Hotfix() {
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
		
		[MenuItem("XLua/Reload Config")]
		private static void ReloadConfig() {
			if( !Application.isPlaying )
				return;

			var luaVm = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			var serviceTable = luaVm.LoadFileAtPath("ConfigService")[0] as LuaTable;
			var func = serviceTable.Get<Action>("Reload");
			func.Invoke();
		}

		private static readonly List<string> modifiedModules = new List<string>();
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

			if( Directory.Exists($"{Environment.CurrentDirectory}\\Lua") == false ) {
				return;
			}
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