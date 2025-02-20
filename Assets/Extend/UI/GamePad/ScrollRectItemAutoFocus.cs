using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Extend.UI.GamePad {
	public class ScrollRectItemAutoFocus : MonoBehaviour, ISelectHandler {
		private ScrollRect m_scrollRect;
		public void OnSelect(BaseEventData eventData) {
			m_scrollRect = GetComponentInParent<ScrollRect>();
			var input = PlayerInput.GetPlayerByIndex(0);
			if( (input != null && input.currentControlScheme == "Gamepad") || eventData is AxisEventData ) {
				m_scrollRect.FocusOnItem(transform as RectTransform);
			}
		}
	}
}