using System.Collections.Generic;
using Extend.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Extend.LuaBindingEvent {
	public class LuaBindingDragEvent : LuaBindingEventBase, IBeginDragHandler, IDragHandler, IEndDragHandler {
		[ReorderList, LabelText("On Drag Start ()"), SerializeField]
		private BindingEvent[] m_dragStartEvent;
		
		[ReorderList, LabelText("On Drag End ()"), SerializeField]
		private BindingEvent[] m_dragEndEvent;
		
		[ReorderList, LabelText("On Drag ()"), SerializeField]
		private BindingEvent[] m_dragEvent;
		
		public void OnBeginDrag(PointerEventData eventData) {
			TriggerPointerEvent("OnBeginDrag", m_dragStartEvent, eventData);
		}

		public void OnDrag(PointerEventData eventData) {
			TriggerPointerEvent("OnDrag", m_dragEvent, eventData);
		}

		public void OnEndDrag(PointerEventData eventData) {
			TriggerPointerEvent("OnEndDrag", m_dragEndEvent, eventData);
		}
	}
}