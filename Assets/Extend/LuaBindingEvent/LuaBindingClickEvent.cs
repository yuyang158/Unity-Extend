using Extend.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Extend.LuaBindingEvent {
	public class LuaBindingClickEvent : LuaBindingEventBase, IPointerClickHandler {
		[ReorderList, LabelText("On Click ()"), SerializeField]
		private BindingEvent[] m_clickEvent;
		
		public void OnPointerClick(PointerEventData eventData) {
			TriggerPointerEvent(m_clickEvent, eventData);
		}
	}
}