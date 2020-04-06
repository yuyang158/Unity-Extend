using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Extend.UI {
	[RequireComponent(typeof(Button))]
	public class UIButton : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IPointerUpHandler {
		public UIAnimation PointerDown;
		public UIAnimation PointerClick;
		public UIAnimation PointerUp;

		private Button button;
		private void Awake() {
			button = GetComponent<Button>();
		}

		public void OnPointerDown(PointerEventData eventData) {
			if( PointerDown.Enabled && button.interactable ) {
				PointerDown.Active(transform);
			}
		}

		public void OnPointerClick(PointerEventData eventData) {
			if( PointerClick.Enabled && button.interactable ) {
				PointerClick.Active(transform);
			}
		}

		public void OnPointerUp(PointerEventData eventData) {
			if( PointerUp.Enabled && button.interactable ) {
				PointerUp.Active(transform);
			}
		}
	}
}