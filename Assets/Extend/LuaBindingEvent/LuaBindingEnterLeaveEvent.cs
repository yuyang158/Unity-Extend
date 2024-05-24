using Extend.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Extend.LuaBindingEvent {
	public class LuaBindingEnterLeaveEvent : LuaBindingEventBase, IPointerEnterHandler, IPointerExitHandler {
		[ReorderList, LabelText("On Enter ()"), SerializeField]
		private BindingEvent[] m_enterEvent;
		[ReorderList, LabelText("On Exit ()"), SerializeField]
		private BindingEvent[] m_exitEvent;

		public void OnPointerEnter(PointerEventData eventData) {
			if( Application.isMobilePlatform ) {
				return;
			}
			TriggerPointerEvent("OnEnter", m_enterEvent, eventData);
		}

		public void OnPointerExit(PointerEventData eventData) {
			if( Application.isMobilePlatform ) {
				return;
			}
			TriggerPointerEvent("OnExit", m_exitEvent, eventData);
		}
	}
}
