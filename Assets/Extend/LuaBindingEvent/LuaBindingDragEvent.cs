using UnityEngine.EventSystems;

namespace Extend.LuaBindingEvent {
	public class LuaBindingDragEvent : LuaBindingEventBase, IBeginDragHandler, IDragHandler, IEndDragHandler {
		[LuaEvents("On Drag Start ()")]
		public LuaBindingEvents DragStartEvent;
		
		[LuaEvents("On Drag End ()")]
		public LuaBindingEvents DragEndEvent;
		
		[LuaEvents("On Drag ()")]
		public LuaBindingEvents DragEvent;
		
		public void OnBeginDrag(PointerEventData eventData) {
			TriggerPointerEvent(DragStartEvent.Events, eventData);
		}

		public void OnDrag(PointerEventData eventData) {
			TriggerPointerEvent(DragEvent.Events, eventData);
		}

		public void OnEndDrag(PointerEventData eventData) {
			TriggerPointerEvent(DragEndEvent.Events, eventData);
		}
	}
}