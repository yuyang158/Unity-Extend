using Extend.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Extend.LuaBindingEvent {
	public class LuaBindingUpDownMoveEvent : LuaBindingEventBase, IPointerDownHandler, IPointerUpHandler, IDragHandler {
		[ReorderList, LabelText("On Down ()"), SerializeField]
		private BindingEvent[] downEvent;
		
		[ReorderList, LabelText("On Up ()"), SerializeField]
		private BindingEvent[] upEvent;
		
		[ReorderList, LabelText("On Move ()"), SerializeField]
		private BindingEvent[] moveEvent;
		
		public void OnPointerDown(PointerEventData eventData) {
			TriggerPointerEvent(downEvent, eventData);
		}

		public void OnPointerUp(PointerEventData eventData) {
			TriggerPointerEvent(upEvent, eventData);
		}

		public void OnDrag(PointerEventData eventData) {
			TriggerPointerEvent(moveEvent, eventData);
		}
	}
}