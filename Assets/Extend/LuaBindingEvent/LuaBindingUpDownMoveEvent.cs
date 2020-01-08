using UnityEngine.EventSystems;

namespace Extend.LuaBindingEvent {
	public class LuaBindingUpDownMoveEvent : LuaBindingEventBase, IPointerDownHandler, IPointerUpHandler, IDragHandler {
		[LuaEvents("On Down ()")]
		public LuaBindingEvents DownEvent;
		
		[LuaEvents("On Up ()")]
		public LuaBindingEvents UpEvent;
		
		[LuaEvents("On Move ()")]
		public LuaBindingEvents MoveEvent;
		
		public void OnPointerDown(PointerEventData eventData) {
			TriggerPointerEvent(DownEvent.Events, eventData);
		}

		public void OnPointerUp(PointerEventData eventData) {
			TriggerPointerEvent(UpEvent.Events, eventData);
		}

		public void OnDrag(PointerEventData eventData) {
			TriggerPointerEvent(MoveEvent.Events, eventData);
		}
	}
}