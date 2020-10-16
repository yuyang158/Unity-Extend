using System;
using System.Collections.Generic;
using Extend.Common;
using Extend.Common.Editor.InspectorGUI;
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

		public enum ConnectionMode {
			None,
			ConnectIDE,
			ListenAndWait
		}

		[SerializeField]
		private ConnectionMode m_connectionMode;

		public ConnectionMode DebugConnectionMode => m_connectionMode;

		[SerializeField]
		private int m_debugPort = 10000;

		public int DebugPort => m_debugPort;

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

	[InitializeOnLoad]
	internal static class LuaMVVMDebugSettingIMGUIRegister {
		private static bool m_debuggerConnected;
		private static LuaFunction m_debugFunc;
		private static List<GameObject> m_mvvmWatchGOs;
		private static ReorderableList m_mvvmWatchGUI;

		static LuaMVVMDebugSettingIMGUIRegister() {
			EditorApplication.playModeStateChanged += mode => {
				var settingObj = LuaDebugSetting.GetOrCreateSettings();
				if( mode == PlayModeStateChange.EnteredPlayMode ) {
					LuaMVVMBindingOption.DebugCheckCallback += go => {
						if( settingObj.MvvmBreakEnabled && m_debuggerConnected && m_mvvmWatchGOs.Contains(go) ) {
							m_debugFunc.Call();
						}
					};
				}
			};

			LuaVM.OnVMCreated += () => {
				m_mvvmWatchGOs = new List<GameObject>();
				m_mvvmWatchGUI = new ReorderableList(m_mvvmWatchGOs, typeof(GameObject)) {
					drawHeaderCallback = rect => { EditorGUI.LabelField(rect, "Watched GameObjects"); },
					drawElementCallback = (rect, index, active, focused) => {
						m_mvvmWatchGOs[index] = EditorGUI.ObjectField(rect, m_mvvmWatchGOs[index],
							typeof(GameObject), true) as GameObject;
					},
					onAddCallback = list => {
						m_mvvmWatchGOs.Add(null);
					},
					onRemoveCallback = list => {
						if(list.index < 0 || list.index >= m_mvvmWatchGOs.Count)
							return;
						m_mvvmWatchGOs.RemoveAt(list.index);
					} 
				};

				m_debuggerConnected = false;
				var luaVm = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
				var tbl = luaVm.LoadFileAtPath("EmmyDebugger")[0] as LuaTable;
				var initFunc = tbl.Get<LuaFunction>("init");
				var connectFunc = tbl.Get<LuaFunction>("connect");
				var listenFunc = tbl.Get<LuaFunction>("listen");
				m_debugFunc = tbl.Get<LuaFunction>("break");

				var settingObj = LuaDebugSetting.GetOrCreateSettings();
				switch( settingObj.DebugConnectionMode ) {
					case LuaDebugSetting.ConnectionMode.None:
						return;
					case LuaDebugSetting.ConnectionMode.ConnectIDE:
					case LuaDebugSetting.ConnectionMode.ListenAndWait:
						if( string.IsNullOrEmpty(settingObj.EmmyCoreLuaSearchPath) ) {
							Debug.LogWarning("If you want to debug lua, set the emmy_core search path.");
							return;
						}

						initFunc.Call(settingObj.EmmyCoreLuaSearchPath);
						if( settingObj.DebugConnectionMode == LuaDebugSetting.ConnectionMode.ListenAndWait ) {
							listenFunc.Call(settingObj.DebugPort);
						}
						else {
							connectFunc.Call(settingObj.DebugPort);
						}

						m_debuggerConnected = true;
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
					var settings = LuaDebugSetting.GetSerializedSettings();
					var debuggerConnectionModeProp = settings.FindProperty("m_connectionMode");
					EditorGUILayout.PropertyField(debuggerConnectionModeProp);

					var mode = (LuaDebugSetting.ConnectionMode)debuggerConnectionModeProp.intValue;
					var debugPortProp = settings.FindProperty("m_debugPort");
					switch( mode ) {
						case LuaDebugSetting.ConnectionMode.None:
							break;
						case LuaDebugSetting.ConnectionMode.ConnectIDE:
							EditorGUILayout.PropertyField(debugPortProp, m_connectPortText);
							break;
						case LuaDebugSetting.ConnectionMode.ListenAndWait:
							EditorGUILayout.PropertyField(debugPortProp, m_listenPortText);
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}

					if( mode != LuaDebugSetting.ConnectionMode.None ) {
						var emmyCoreLuaSearchPathProp = settings.FindProperty("m_emmyCoreLuaSearchPath");
						EditorGUILayout.PropertyField(emmyCoreLuaSearchPathProp);
					}

					if( debugPortProp.intValue < 10000 ) {
						debugPortProp.intValue = 10000;
					}

					var enabledProp = settings.FindProperty("m_mvvmBreakEnabled");
					EditorGUILayout.PropertyField(enabledProp);

					if( Application.isPlaying ) {
						m_mvvmWatchGUI.DoLayoutList();
					}

					settings.ApplyModifiedProperties();
				}
			};
			return provider;
		}
	}
}