using UnityEngine.EventSystems;

namespace Extend.LuaBindingEvent.RoutedEvent {
	public class UpwardClickEvent : UpwardEventBase, IPointerClickHandler {
		public void OnPointerClick(PointerEventData eventData) {
			RouteEvent("OnClick", eventData);
		}
	}
}