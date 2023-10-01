using Extend.Common;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Extend.LuaBindingEvent {
	[RequireComponent(typeof(TMP_Text))]
	public class LuaBindingTextLinkEvent : LuaBindingEventBase, IPointerExitHandler, IPointerMoveHandler, IPointerClickHandler {
		[ReorderList, LabelText("On Link ()"), SerializeField]
		private BindingEvent[] m_linkEvent;
		
		[ReorderList, LabelText("On Link Click()"), SerializeField]
		private BindingEvent[] m_linkClickEvent;

		private TMP_Text m_text;
		protected override void Awake() {
			base.Awake();
			m_text = GetComponent<TMP_Text>();
		}

		public void OnPointerExit(PointerEventData eventData) {
			TriggerPointerEvent("OnLink", m_linkEvent, null);
		}

		public void OnPointerMove(PointerEventData eventData) {
			var linkIndex = TMP_TextUtilities.FindIntersectingLink(m_text, eventData.position, eventData.enterEventCamera);
			if( linkIndex == -1 ) {
				TriggerPointerEvent("OnLink", m_linkEvent, null);
			}
			else {
				var link = m_text.textInfo.linkInfo[linkIndex];
				TriggerPointerEvent("OnLink", m_linkEvent, link.GetLinkID());
			}
		}

		public void OnPointerClick(PointerEventData eventData) {
			var linkIndex = TMP_TextUtilities.FindIntersectingLink(m_text, eventData.position, eventData.enterEventCamera);
			if( linkIndex != -1 ) {
				var link = m_text.textInfo.linkInfo[linkIndex];
				TriggerPointerEvent("OnLinkClick", m_linkClickEvent, link.GetLinkID());
			}
		}
	}
}
