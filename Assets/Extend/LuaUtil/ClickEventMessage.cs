using Extend.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Extend.LuaUtil {
	public class ClickEventMessage : MonoBehaviour, IPointerClickHandler {
		public string Message;
		
		public void OnPointerClick(PointerEventData eventData) {
			var luaVm = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			luaVm.SendCSharpMessage(Message, eventData);
		}
	}
}