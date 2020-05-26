using System.IO;
using Extend.Asset;
using Extend.Common;
using UnityEngine;
using XLua;

namespace Extend {
	[LuaCallCSharp]
	public class GameSystem : IService {
		public IniRead SystemSetting { get; private set; }
		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.GAME_SYSTEM_SERVICE;

		public static GameSystem Get() {
			return CSharpServiceManager.Get<GameSystem>(CSharpServiceManager.ServiceType.GAME_SYSTEM_SERVICE);
		}
		public void Initialize() {
			const string fileName = "SystemSetting";
			if( Application.isEditor ) {
				var path = Path.Combine(Application.persistentDataPath, fileName + ".ini");
				if( !File.Exists(path) ) {
					using( var asset = AssetService.Get().Load($"Config/{fileName}", typeof(TextAsset)) ) {
						File.WriteAllText(path, asset.GetTextAsset().text);
					}
				}
				using( var reader = new StreamReader(path) ) {
					SystemSetting = IniRead.Parse(reader);
				}
			}
			using( var asset = AssetService.Get().Load($"Config/{fileName}", typeof(TextAsset)) ) {
				using( var reader = new StringReader(asset.GetTextAsset().text) ) {
					SystemSetting = IniRead.Parse(reader);
				}
			}
		}

		public void Destroy() {
		}
	}
}