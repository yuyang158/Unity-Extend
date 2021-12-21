using Extend.Common;
using UnityEngine;

namespace Extend {
	public class AppStartUp : MonoBehaviour {
		private void Start() {
			var lua = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			lua.StartUp();
		}
	}
}