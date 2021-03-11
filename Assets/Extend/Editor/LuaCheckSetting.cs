using UnityEditor;
using UnityEngine;

namespace Extend.Editor {
	public class LuaCheckSetting : ScriptableObject {
		public const string SETTING_PATH = "Assets/Extend/Editor/LuaCheckSetting.asset";

		[SerializeField]
		private string m_luaCheckExecPath;

		[SerializeField]
		private bool m_active;

		public string LuaCheckExecPath => m_luaCheckExecPath;

		public bool Active => m_active;

		[SerializeField, Range(100, 250)]
		private int m_maxLineLength = 140;

		public int MaxLineLength => m_maxLineLength;

		internal static LuaCheckSetting GetOrCreateSettings() {
			var settings = AssetDatabase.LoadAssetAtPath<LuaCheckSetting>(SETTING_PATH);
			if( !settings ) {
				settings = CreateInstance<LuaCheckSetting>();
				AssetDatabase.CreateAsset(settings, SETTING_PATH);
				AssetDatabase.SaveAssets();
			}

			return settings;
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
					settings.ApplyModifiedProperties();
				}
			};
			return provider;
		}
	}
}