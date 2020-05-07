using Extend.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Extend.LuaBindingEvent {
	public class LuaBindingClickEvent : LuaBindingEventBase, IPointerClickHandler {
		[ReorderList, LabelText("On Click ()"), SerializeField]
		private BindingEvent[] ClickEvent;
		
		public void OnPointerClick(PointerEventData eventData) {
			TriggerPointerEvent(ClickEvent, eventData);
		}
	}
}