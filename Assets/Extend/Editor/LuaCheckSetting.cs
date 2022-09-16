using UnityEditor;
using UnityEngine;

namespace Extend.Editor {
	public class LuaCheckSetting : ScriptableObject {
		[SerializeField]
		private string m_luaCheckExecPath;

		[SerializeField]
		private bool m_active;

		public string LuaCheckExecPath => m_luaCheckExecPath;

		public bool Active => m_active;

		[SerializeField, Range(100, 250)]
		private int m_maxLineLength = 140;

		public int MaxLineLength => m_maxLineLength;
		private static LuaCheckSetting m_setting;

		internal static LuaCheckSetting GetOrCreateSettings() {
			if( !m_setting ) {
				var setting = CreateInstance<LuaCheckSetting>();
				setting.m_active = EditorPrefs.GetBool("LuaCheckSetting.m_active", false);
				setting.m_luaCheckExecPath = EditorPrefs.GetString("LuaCheckSetting.m_luaCheckExecPath", "");
				setting.m_maxLineLength = EditorPrefs.GetInt("LuaCheckSetting.m_maxLineLength", 140);
				m_setting = setting;
			}

			return m_setting;
		}

		internal static void Save() {
			EditorPrefs.SetBool("LuaCheckSetting.m_active", m_setting.m_active);
			EditorPrefs.SetString("LuaCheckSetting.m_luaCheckExecPath", m_setting.m_luaCheckExecPath);
			EditorPrefs.SetInt("LuaCheckSetting.m_maxLineLength", m_setting.m_maxLineLength);
		}

		internal static SerializedObject GetSerializedSettings() {
			return new SerializedObject(GetOrCreateSettings());
		}
	}

	internal static class LuaCheckSettingIMGUIRegister {
		[SettingsProvider]
		public static SettingsProvider CreateLuaCheckSettingProvider() {
			var provider = new SettingsProvider("Project/LuaCheckSetting", SettingsScope.Project) {
				label = "Setup Lua Check",
				guiHandler = search => {
					var settings = LuaCheckSetting.GetSerializedSettings();
					EditorGUI.BeginChangeCheck();
					var activeProp = settings.FindProperty("m_active");
					EditorGUILayout.PropertyField(activeProp);

					EditorGUILayout.BeginHorizontal();
					GUI.enabled = false;
					var execPathProp = settings.FindProperty("m_luaCheckExecPath");
					EditorGUILayout.PropertyField(execPathProp);
					GUI.enabled = true;
					if( GUILayout.Button("Open", "LargeButtonRight", GUILayout.Width(80)) ) {
						var selected = EditorUtility.OpenFilePanel("Select LuaCheck.exe", "", "exe");
						execPathProp.stringValue = selected;
					}
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.PropertyField(settings.FindProperty("m_maxLineLength"));
					if( EditorGUI.EndChangeCheck() ) {
						settings.ApplyModifiedProperties();
						LuaCheckSetting.Save();
					}
				}
			};
			return provider;
		}
	}
}