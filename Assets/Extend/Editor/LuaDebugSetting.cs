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
		private bool m_enabled;
		public bool Enabled => m_enabled;

		[SerializeField]
		private string m_emmyCoreLuaSearchPath;
		public string EmmyCoreLuaSearchPath;

		[SerializeField]
		private GameObject[] m_watchedGOs;
		public GameObject[] WatchedGOs => m_watchedGOs;
		private ReorderableList m_guiWatchedList;

		public ReorderableList GUIWatchedList {
			get {
				if( m_guiWatchedList == null ) {
					var slObj = GetSerializedSettings();
					var watchedGOsProp = slObj.FindProperty("m_watchedGOs");
					m_guiWatchedList = new ReorderableList(slObj, watchedGOsProp);
				}

				return m_guiWatchedList;
			}
		}

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
		[SettingsProvider]
		public static SettingsProvider CreateLuaCheckSettingProvider() {
			EditorApplication.playModeStateChanged += mode => {
				var settingObj = LuaDebugSetting.GetOrCreateSettings();
				if( mode == PlayModeStateChange.EnteredPlayMode ) {
					LuaMVVMBindingOption.DebugCheckCallback += go => {
						if( settingObj.Enabled && ArrayUtility.IndexOf(settingObj.WatchedGOs, go) >= 0 ) {
							var luaVm = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
							luaVm.Global.GetInPath<LuaFunction>("");
						}
					};
				}
			};
			
			var provider = new SettingsProvider("Project/Lua Debug", SettingsScope.Project) {
				label = "Lua Debug Setup",
				guiHandler = search => {
					var settingObj = LuaDebugSetting.GetOrCreateSettings();
					var settings = LuaDebugSetting.GetSerializedSettings();
					var enabledProp = settings.FindProperty("m_enabled");
					EditorGUILayout.PropertyField(enabledProp);

					if( Application.isPlaying ) {
						var goProp = settings.FindProperty("m_watchedGOs");
						for( var i = 0; i < goProp.arraySize; ) {
							var elementProp = goProp.GetArrayElementAtIndex(i);
							if( elementProp.objectReferenceValue == null ) {
								goProp.DeleteArrayElementAtIndex(i);
							}
							else {
								i++;
							}
						}
						settingObj.GUIWatchedList.DoLayoutList();
					}
					settings.ApplyModifiedProperties();
				}
			};
			return provider;
		}
	}
}