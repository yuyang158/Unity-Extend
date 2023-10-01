using System.Collections.Generic;
using Extend.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Extend.LuaBindingEvent {
	public class LuaBindingUpwardEventReceiver : LuaBindingEventBase {
		[ReorderList, LabelText("On Routed Event ()"), SerializeField]
		private BindingEvent[] m_event;
		
		public void OnEvent(string eventName, PointerEventData eventData) {
			TriggerPointerEvent(eventName, m_event, eventData);
		}
	}
}