using System;
using System.Collections.Generic;
using Extend.Common;
using Extend.LuaMVVM;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityToolbarExtender;
using XLua;

namespace Extend.Editor {
	public class LuaDebugSetting : ScriptableObject {
		[SerializeField]
		private bool m_mvvmBreakEnabled;

		public bool MvvmBreakEnabled => m_mvvmBreakEnabled;

		public enum DebugMode {
			None,
			ConnectIDE,
			ListenAndWait,
			CoverageMode
		}

		[SerializeField]
		private DebugMode m_debugMode;

		public DebugMode Mode => m_debugMode;

		[SerializeField]
		private int m_debugPort = 10000;

		public int DebugPort => m_debugPort;

		[SerializeField, ReorderList]
		private string[] m_coverageCheckFiles;

		public string[] CoverageCheckFiles => m_coverageCheckFiles;
		private static LuaDebugSetting m_setting;

		public static LuaDebugSetting GetOrCreateSettings() {
			if( !m_setting ) {
				m_setting = CreateInstance<LuaDebugSetting>();
				m_setting.m_debugMode = (DebugMode)EditorPrefs.GetInt("LuaDebugSetting.m_debugMode", (int)DebugMode.None);
				m_setting.m_debugPort = EditorPrefs.GetInt("LuaDebugSetting.m_debugPort", 10000);
			}

			return m_setting;
		}

		internal static SerializedObject GetSerializedSettings() {
			return new SerializedObject(GetOrCreateSettings());
		}

		internal static void Save() {
			EditorPrefs.SetInt("LuaDebugSetting.m_debugMode", (int)m_setting.m_debugMode);
			EditorPrefs.SetInt("LuaDebugSetting.m_debugPort", m_setting.m_debugPort);
		}
	}

	[InitializeOnLoad]
	internal static class LuaMVVMDebugSettingIMGUIRegister {
		private static bool m_debuggerConnected;
		private static LuaFunction m_debugFunc;
		private static List<GameObject> m_mvvmWatchGOs;
		private static ReorderableList m_mvvmWatchGUI;

		[InitializeOnEnterPlayMode]
		private static void Setup() {
			var settingObj = LuaDebugSetting.GetOrCreateSettings();
			LuaMVVMBindingOption.DebugCheckCallback = go => {
				if( settingObj.MvvmBreakEnabled && m_debuggerConnected && m_mvvmWatchGOs.Contains(go) ) {
					m_debugFunc.Call();
				}
			};

			LuaVM.OnVMQuiting += () => {
				var luaVm = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
				if( settingObj.Mode == LuaDebugSetting.DebugMode.CoverageMode ) {
					var luaCovRunner = luaVm.LoadFileAtPath("luacov.runner")[0] as LuaTable;
					var runnerShutdownFunc = luaCovRunner.GetInPath<LuaFunction>("shutdown");
					runnerShutdownFunc.Call();
					return;
				}

				if( settingObj.Mode == LuaDebugSetting.DebugMode.None )
					return;

				var tbl = luaVm.LoadFileAtPath("EmmyDebuggerBridge")[0] as LuaTable;
				var stop = tbl.Get<LuaFunction>("stop");
				if( stop == null )
					return;
				stop.Call();
			};

			LuaVM.OnVMCreated += () => {
				m_mvvmWatchGOs = new List<GameObject>();
				m_mvvmWatchGUI = new ReorderableList(m_mvvmWatchGOs, typeof(GameObject)) {
					drawHeaderCallback = rect => { EditorGUI.LabelField(rect, "Watched GameObjects"); },
					drawElementCallback = (rect, index, active, focused) => {
						m_mvvmWatchGOs[index] = EditorGUI.ObjectField(rect, m_mvvmWatchGOs[index],
							typeof(GameObject), true) as GameObject;
					},
					onAddCallback = _ => { m_mvvmWatchGOs.Add(null); },
					onRemoveCallback = list => {
						if( list.index < 0 || list.index >= m_mvvmWatchGOs.Count )
							return;
						m_mvvmWatchGOs.RemoveAt(list.index);
					}
				};

				m_debuggerConnected = false;
				var luaVm = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
				var tbl = luaVm.LoadFileAtPath("EmmyDebuggerBridge")[0] as LuaTable;
				var initFunc = tbl.Get<LuaFunction>("init");
				var connectFunc = tbl.Get<LuaFunction>("connect");
				var listenFunc = tbl.Get<LuaFunction>("listen");
				m_debugFunc = tbl.Get<LuaFunction>("break");

				switch( settingObj.Mode ) {
					case LuaDebugSetting.DebugMode.None:
						return;
					case LuaDebugSetting.DebugMode.ConnectIDE:
					case LuaDebugSetting.DebugMode.ListenAndWait:
						#if UNITY_EDITOR_WIN
						var appDataPath = Environment.GetEnvironmentVariable("APPDATA");
						string searchPath = $";{appDataPath}/JetBrains/Rider2023.2/plugins/EmmyLua/debugger/emmy/windows/x64/?.dll";
						#elif UNITY_EDITOR_OSX
						const string searchPath = ";/Users/yuyang/.vscode/extensions/tangzx.emmylua-0.5.19/debugger/emmy/mac/x64/?.dylib";
						#endif
						initFunc.Call(searchPath);
						if( settingObj.Mode == LuaDebugSetting.DebugMode.ListenAndWait ) {
							listenFunc.Call(settingObj.DebugPort);
						}
						else {
							connectFunc.Call(settingObj.DebugPort);
						}
						m_debuggerConnected = true;
						break;
					case LuaDebugSetting.DebugMode.CoverageMode:
						var luaCovRunner = luaVm.LoadFileAtPath("luacov.runner")[0] as LuaTable;
						var runnerInitFunc = luaCovRunner.GetInPath<LuaFunction>("init");
						var configuration = luaVm.NewTable();
						configuration.SetInPath("runreport", true);
						var includedFiles = luaVm.NewTable();
						for( int i = 0; i < settingObj.CoverageCheckFiles.Length; i++ ) {
							var file = settingObj.CoverageCheckFiles[i];
							includedFiles.Set(i + 1, file);
						}

						configuration.SetInPath("include", includedFiles);
						configuration.SetInPath("reportfile", "D:/test.stat");
						runnerInitFunc.Call(configuration);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			};
		}

		private static readonly GUIContent m_connectPortText = new GUIContent("Connect Port");
		private static readonly GUIContent m_listenPortText = new GUIContent("Listen Port");

		static LuaMVVMDebugSettingIMGUIRegister() {
			ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
		}

		private static void OnToolbarGUI() {
			EditorGUI.BeginChangeCheck();
			var settings = LuaDebugSetting.GetSerializedSettings();
			var debuggerConnectionModeProp = settings.FindProperty("m_debugMode");
			EditorGUIUtility.labelWidth = 80;
			EditorGUILayout.PropertyField(debuggerConnectionModeProp, GUILayout.Width(200));
			if( EditorGUI.EndChangeCheck() ) {
				settings.ApplyModifiedProperties();
				LuaDebugSetting.Save();
			}
		}

		[SettingsProvider]
		public static SettingsProvider CreateLuaCheckSettingProvider() {
			var provider = new SettingsProvider("Project/Lua Debug", SettingsScope.Project) {
				label = "Setup Lua Debug",
				guiHandler = search => {
					EditorGUI.indentLevel = 1;
					EditorGUI.BeginChangeCheck();
					var settings = LuaDebugSetting.GetSerializedSettings();
					var debuggerConnectionModeProp = settings.FindProperty("m_debugMode");
					EditorGUILayout.PropertyField(debuggerConnectionModeProp);

					var mode = (LuaDebugSetting.DebugMode)debuggerConnectionModeProp.intValue;
					var debugPortProp = settings.FindProperty("m_debugPort");
					switch( mode ) {
						case LuaDebugSetting.DebugMode.None:
							break;
						case LuaDebugSetting.DebugMode.ConnectIDE:
							EditorGUILayout.PropertyField(debugPortProp, m_connectPortText);
							break;
						case LuaDebugSetting.DebugMode.ListenAndWait:
							EditorGUILayout.PropertyField(debugPortProp, m_listenPortText);
							break;
						case LuaDebugSetting.DebugMode.CoverageMode:
							var coverageCheckFilesProp = settings.FindProperty("m_coverageCheckFiles");
							EditorGUILayout.PropertyField(coverageCheckFilesProp);
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}

					if( debugPortProp.intValue < 10000 ) {
						debugPortProp.intValue = 10000;
					}

					var enabledProp = settings.FindProperty("m_mvvmBreakEnabled");
					EditorGUILayout.PropertyField(enabledProp);

					if( Application.isPlaying ) {
						m_mvvmWatchGUI?.DoLayoutList();
					}

					if( EditorGUI.EndChangeCheck() ) {
						settings.ApplyModifiedProperties();
						LuaDebugSetting.Save();
					}
				}
			};
			return provider;
		}
	}
}
