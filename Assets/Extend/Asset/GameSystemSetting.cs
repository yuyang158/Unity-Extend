using System.Collections.Generic;
using System.IO;
using Extend.Asset;
using Extend.Common;
using UnityEngine;
using XLua;

namespace Extend {
	[LuaCallCSharp]
	public class GameSystemSetting : IService {
		private readonly Dictionary<string, string> m_globalValues = new Dictionary<string, string>();
		public IniRead SystemSetting { get; private set; }
		public int ServiceType => (int)CSharpServiceManager.ServiceType.GAME_SYSTEM_SERVICE;

		public static GameSystemSetting Get() {
			return CSharpServiceManager.Get<GameSystemSetting>(CSharpServiceManager.ServiceType.GAME_SYSTEM_SERVICE);
		}

		public void SetValue(string key, string value) {
			m_globalValues.Add(key, value);
		}

		public string GetValue(string key) {
			if( m_globalValues.TryGetValue(key, out var val) )
				return val;

			Debug.LogWarning($"Not found key in global values : {key}");
			return string.Empty;
		}


		[BlackList]
		public void Initialize() {
			#if XIAOICE_PROD
			const string fileName = "SystemSetting_Prod";
			#else
			const string fileName = "SystemSetting";
			#endif
			var path = Path.Combine(Application.persistentDataPath, fileName + ".ini");
			CopyToPersistent(fileName, path);

			using( var reader = new StreamReader(path) ) {
				SystemSetting = IniRead.Parse(reader);
			}
			
			var streamingAssetFile = FileLoader.LoadFileSync($"Config/{fileName}.ini");
			using( var internalReader = new StreamReader(streamingAssetFile) ) {
				var streamingAssetSetting = IniRead.Parse(internalReader);
				if( SystemSetting.GetString("Game", "SettingVersion") != streamingAssetSetting.GetString("Game", "SettingVersion") ) {
					CopyToPersistent(fileName, path);
					using( var reader = new StreamReader(path) ) {
						SystemSetting = IniRead.Parse(reader);
					}
				}
			}
			IniRead.SystemSetting = SystemSetting;
		}

		private static void CopyToPersistent(string fileName, string persistent) {
			using( var stream = FileLoader.LoadFileSync($"Config/{fileName}.ini", false) ) {
				var buffer = new byte[stream.Length];
				stream.Read(buffer, 0, (int) stream.Length);
				File.WriteAllBytes(persistent, buffer);
			}
		}

		[BlackList]
		public void Destroy() {
		}
	}
}