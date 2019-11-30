using UnityEngine.EventSystems;

namespace Extend.LuaBindingEvent {
	public class LuaBindingClickEvent : LuaBindingEventBase, IPointerClickHandler {
		[LuaEvents("On Click ()")]
		public LuaBindingEvents ClickEvent;
		
		public void OnPointerClick(PointerEventData eventData) {
			TriggerPointerEvent(ClickEvent.Events, eventData);
		}
	}
}