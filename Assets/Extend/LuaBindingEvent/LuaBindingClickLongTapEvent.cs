using System.Collections.Generic;
using Extend.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Extend.LuaBindingEvent {
	public class LuaBindingClickLongTapEvent : LuaBindingEventBase , IPointerDownHandler, IPointerUpHandler,
		/*IPointerExitHandler,*/ IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		[ReorderList, LabelText("On Long Tap ()"), SerializeField]
		private BindingEvent[] m_longTapEvent;
		[ReorderList, LabelText("On Click ()"), SerializeField]
		private BindingEvent[] m_clickEvent;
		public float LongTapTime = 1f;
		public ScrollRect ScrollRect;
		float m_downTime;
		bool m_down = false;

		void Update()
		{
			if (!m_down) return;
			if (Time.time - m_downTime > LongTapTime)
			{
				TriggerPointerEvent("OnLongTap", m_longTapEvent, null);
				m_down = false;
			}
		}
		public void OnPointerDown(PointerEventData eventData)
		{
			m_downTime = Time.time;
			m_down = true;
		}
		public void OnPointerUp(PointerEventData eventData)
		{
			if (!m_down && !eventData.dragging) return;
			if (Time.time - m_downTime < LongTapTime)
			{
				TriggerPointerEvent("OnClick", m_clickEvent, eventData);
				m_down = false;
			}
		}

		//public void OnPointerExit(PointerEventData eventData)
		//{
		//	m_down = false;
		//}
		public void OnBeginDrag(PointerEventData eventData)
		{
			m_down = false;
			ScrollRect?.OnBeginDrag(eventData);
		}
		public void OnDrag(PointerEventData eventData)
		{
			ScrollRect?.OnDrag(eventData);
		}
		public void OnEndDrag(PointerEventData eventData)
		{
			ScrollRect?.OnEndDrag(eventData);
		}
	}
}