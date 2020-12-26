using System.Collections.Generic;
using Extend.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Extend.LuaBindingEvent {
	public class LuaBindingClickEvent : LuaBindingEventBase, IPointerClickHandler {
		[ReorderList, LabelText("On Click ()"), SerializeField]
		private List<BindingEvent> m_clickEvent;
		
		public void OnPointerClick(PointerEventData eventData) {
			TriggerPointerEvent("OnClick", m_clickEvent, eventData);
		}
	}
}