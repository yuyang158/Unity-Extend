using Extend.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Extend.LuaBindingEvent {
	public class LuaBindingPressAndHoldEvent : LuaBindingEventBase, IBeginDragHandler, IPointerDownHandler, IPointerUpHandler {
		[SerializeField]
		private float m_holdDelay = 1.5f;
		[SerializeField]
		private float m_holdDuration = 1.5f;

		[SerializeField]
		private GameObject m_pressFX;

		private GameObject m_progressFxGo;
		private Image m_progressImg;
		[ReorderList, LabelText("On Hold ()"), SerializeField]
		private BindingEvent[] m_holdEvent;

		[SerializeField]
		private bool m_ignoreOnDesktop;

		private bool m_pressed;

		private bool Pressed {
			set {
				m_pressed = value;
				if( !m_pressed && m_progressFxGo ) {
					m_progressFxGo.SetActive(false);
				}
			}
		}
		
		public void OnBeginDrag(PointerEventData eventData) {
			Pressed = false;
		}

		public void OnPointerDown(PointerEventData eventData) {
			if( !Application.isMobilePlatform ) {
				return;
			}
			m_timeLast = 0;
			Pressed = true;
			var pressedTarget = eventData.pointerEnter.transform as RectTransform;
			var canvas = pressedTarget.GetComponentInParent<Canvas>();
			if( RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, eventData.position,
				   eventData.pressEventCamera, out var localPoint) ) {
				var rectTransform = m_progressFxGo.transform as RectTransform;
				rectTransform.SetParent(canvas.transform, false);
				rectTransform.localPosition = localPoint;
			}
		}

		public void OnPointerUp(PointerEventData eventData) {
			Pressed = false;
		}

		protected override void Awake() {
			m_progressFxGo = Instantiate(m_pressFX);
			m_progressFxGo.SetActive(false);
			m_progressImg = m_progressFxGo.GetComponent<Image>();

			if( !Application.isMobilePlatform && !m_ignoreOnDesktop ) {
				var button = GetComponent<Button>();
				button.onClick.AddListener(() => {
					TriggerPointerEvent("OnHold", m_holdEvent, null);
				});
			}
		}

		private float m_timeLast;
		private void Update() {
			if( !m_pressed ) {
				return;
			}

			var oldTime = m_timeLast;
			m_timeLast += Time.deltaTime;
			if( m_timeLast < m_holdDelay ) {
				return;
			}

			if( oldTime < m_holdDelay ) {
				m_progressFxGo.SetActive(true);
			}
			var time = m_timeLast - m_holdDelay;
			var progress = time / m_holdDuration;
			m_progressImg.fillAmount = Mathf.Min(1, progress);
			if( progress > 1 ) {
				Pressed = false;
				m_progressFxGo.transform.SetParent(null, false);
				TriggerPointerEvent("OnHold", m_holdEvent, null);
			}
		}

		private void OnDestroy() {
			Destroy(m_progressFxGo);
		}
	}
}
