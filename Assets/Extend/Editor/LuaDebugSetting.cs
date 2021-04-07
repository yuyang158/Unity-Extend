using System;
using System.Collections.Generic;
using Extend.Common;
using Extend.LuaMVVM;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using XLua;

namespace Extend.Editor {
	public class LuaDebugSetting : ScriptableObject {
		private const string SETTING_PATH = "Assets/Extend/Editor/LuaDebugSetting.asset";

		[SerializeField]
		private bool m_mvvmBreakEnabled;

		public bool MvvmBreakEnabled => m_mvvmBreakEnabled;

		[SerializeField]
		private string m_emmyCoreLuaSearchPath;

		public string EmmyCoreLuaSearchPath => m_emmyCoreLuaSearchPath;

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

		public static LuaDebugSetting GetOrCreateSettings() {
			var settings = AssetDatabase.LoadAssetAtPath<LuaDebugSetting>(SETTING_PATH);
			if( !settings ) {
				settings = CreateInstance<LuaDebugSetting>();
				AssetDatabase.CreateAsset(settings, SETTING_PATH);
				AssetDatabase.SaveAssets();
			}

			return settings;
		}

		internal static SerializedObject GetSerializedSettings() {
			return new SerializedObject(GetOrCreateSettings());
		}
	}

	internal static class LuaMVVMDebugSettingIMGUIRegister {
		private static bool m_debuggerConnected;
		private static LuaFunction m_debugFunc;
		private static List<GameObject> m_mvvmWatchGOs;
		private static ReorderableList m_mvvmWatchGUI;

		[InitializeOnEnterPlayMode]
		private static void Setup() {
			var settingObj = LuaDebugSetting.GetOrCreateSettings();
			LuaMVVMBindingOption.DebugCheckCallback += go => {
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
					onAddCallback = list => { m_mvvmWatchGOs.Add(null); },
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
						if( string.IsNullOrEmpty(settingObj.EmmyCoreLuaSearchPath) ) {
							Debug.LogWarning("If you want to debug lua, set the emmy_core search path.");
							return;
						}

						initFunc.Call(settingObj.EmmyCoreLuaSearchPath);
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

		[SettingsProvider]
		public static SettingsProvider CreateLuaCheckSettingProvider() {
			var provider = new SettingsProvider("Project/Lua Debug", SettingsScope.Project) {
				label = "Setup Lua Debug",
				guiHandler = search => {
					EditorGUI.indentLevel = 1;
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

					if( mode != LuaDebugSetting.DebugMode.None ) {
						var emmyCoreLuaSearchPathProp = settings.FindProperty("m_emmyCoreLuaSearchPath");
						EditorGUILayout.PropertyField(emmyCoreLuaSearchPathProp);
					}

					if( debugPortProp.intValue < 10000 ) {
						debugPortProp.intValue = 10000;
					}

					var enabledProp = settings.FindProperty("m_mvvmBreakEnabled");
					EditorGUILayout.PropertyField(enabledProp);

					if( Application.isPlaying ) {
						m_mvvmWatchGUI?.DoLayoutList();
					}

					settings.ApplyModifiedProperties();
				}
			};
			return provider;
		}
	}
}