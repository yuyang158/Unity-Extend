using Extend.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Extend.LuaBindingEvent {
	public class LuaBindingDragEvent : LuaBindingEventBase, IBeginDragHandler, IDragHandler, IEndDragHandler {
		[ReorderList, LabelText("On Drag Start ()"), SerializeField]
		private BindingEvent[] dragStartEvent;
		
		[ReorderList, LabelText("On Drag End ()"), SerializeField]
		private BindingEvent[] dragEndEvent;
		
		[ReorderList, LabelText("On Drag ()"), SerializeField]
		private BindingEvent[] dragEvent;
		
		public void OnBeginDrag(PointerEventData eventData) {
			TriggerPointerEvent(dragStartEvent, eventData);
		}

		public void OnDrag(PointerEventData eventData) {
			TriggerPointerEvent(dragEvent, eventData);
		}

		public void OnEndDrag(PointerEventData eventData) {
			TriggerPointerEvent(dragEndEvent, eventData);
		}
	}
}