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
			using( var asset = AssetService.Get().Load("Config/SystemSetting", typeof(TextAsset)) ) {
				using( var reader = new StringReader(asset.GetTextAsset().text) ) {
					SystemSetting = IniRead.Parse(reader);
				}
			}
		}

		public void Destroy() {
		}
	}
}