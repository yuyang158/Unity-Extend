using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Extend.UI {
	public class UIButton : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IPointerUpHandler {
		public UIAnimation PointerDown;
		public UIAnimation PointerClick;
		public UIAnimation PointerUp;

		public void OnPointerDown(PointerEventData eventData) {
			if( PointerDown.Enabled ) {
				PointerDown.Active(transform);
			}
		}

		public void OnPointerClick(PointerEventData eventData) {
			if( PointerClick.Enabled ) {
				PointerClick.Active(transform);
			}
		}

		public void OnPointerUp(PointerEventData eventData) {
			if( PointerUp.Enabled ) {
				PointerUp.Active(transform);
			}
		}
	}
}