using System.Collections.Generic;
using System.IO;
using Extend.Asset;
using Extend.Common;
using UnityEngine;
using XLua;

namespace Extend {
	[LuaCallCSharp]
	public class GameSystem : IService {
		private readonly Dictionary<string, string> m_globalValues = new Dictionary<string, string>();
		public IniRead SystemSetting { get; private set; }
		public int ServiceType => (int)CSharpServiceManager.ServiceType.GAME_SYSTEM_SERVICE;

		public static GameSystem Get() {
			return CSharpServiceManager.Get<GameSystem>(CSharpServiceManager.ServiceType.GAME_SYSTEM_SERVICE);
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
			const string fileName = "SystemSetting";
#if !UNITY_EDITOR
			var path = Path.Combine(Application.persistentDataPath, fileName + ".ini");
			if( !File.Exists(path) ) {
				using( var stream = FileLoader.LoadFileSync($"Config/{fileName}.ini") ) {
					var buffer = new byte[stream.Length];
					stream.Read(buffer, 0, (int)stream.Length);
					File.WriteAllBytes(path, buffer);
				}
			}

			using( var reader = new StreamReader(path) ) {
				SystemSetting = IniRead.Parse(reader);
			}
#else
			using( var stream = FileLoader.LoadFileSync($"Config/{fileName}.ini") ) {
				using( var reader = new StreamReader(stream) ) {
					SystemSetting = IniRead.Parse(reader);
				}
			}
#endif
			IniRead.SystemSetting = SystemSetting;
		}

		[BlackList]
		public void Destroy() {
		}
	}
}