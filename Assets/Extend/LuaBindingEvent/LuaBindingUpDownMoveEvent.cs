using Extend.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Extend.LuaBindingEvent {
	public class LuaBindingUpDownMoveEvent : LuaBindingEventBase, IPointerDownHandler, IPointerUpHandler, IDragHandler {
		[ReorderList, LabelText("On Down ()"), SerializeField]
		private BindingEvent[] m_downEvent;
		
		[ReorderList, LabelText("On Up ()"), SerializeField]
		private BindingEvent[] m_upEvent;
		
		[ReorderList, LabelText("On Move ()"), SerializeField]
		private BindingEvent[] m_moveEvent;
		
		public void OnPointerDown(PointerEventData eventData) {
			TriggerPointerEvent(m_downEvent, eventData);
		}

		public void OnPointerUp(PointerEventData eventData) {
			TriggerPointerEvent(m_upEvent, eventData);
		}

		public void OnDrag(PointerEventData eventData) {
			TriggerPointerEvent(m_moveEvent, eventData);
		}
	}
}