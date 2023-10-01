using Extend.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Extend.LuaBindingEvent {
	public class LuaBindingFingerClickEvent : LuaBindingEventBase, IPointerClickHandler {
		[ReorderList, LabelText("On Click ()"), SerializeField]
		private BindingEvent[] m_clickEvent;

		public void OnPointerClick() {
			TriggerPointerEvent("OnClick", m_clickEvent, null);
		}

		public void OnPointerClick(PointerEventData eventData) {
			OnPointerClick();
		}
	}
}