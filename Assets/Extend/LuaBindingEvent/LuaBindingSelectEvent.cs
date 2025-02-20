using Extend.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Extend.LuaBindingEvent {
	public class LuaBindingSelectEvent : LuaBindingEventBase, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler {
		[ReorderList, LabelText("On Select ()"), SerializeField]
		private BindingEvent[] m_selectEvent;
		[ReorderList, LabelText("On Deselect ()"), SerializeField]
		private BindingEvent[] m_deselectEvent;

		[SerializeField]
		private bool m_triggerWithPointer = true;

		public void OnSelect(BaseEventData eventData) {
			TriggerPointerEvent("OnSelect", m_selectEvent, eventData);
		}

		public void OnPointerEnter(PointerEventData eventData) {
			if( m_triggerWithPointer ) {
				eventData.selectedObject = gameObject;
				TriggerPointerEvent("OnSelect", m_selectEvent, eventData);
			}
		}

		public void OnPointerExit(PointerEventData eventData) {
			if( m_triggerWithPointer ) {
				eventData.selectedObject = null;
				TriggerPointerEvent("OnDeselect", m_deselectEvent, eventData);
			}
		}

		public void OnDeselect(BaseEventData eventData) {
			TriggerPointerEvent("OnDeselect", m_deselectEvent, eventData);
		}
	}
} 