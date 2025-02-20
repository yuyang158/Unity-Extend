using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Extend.UI.GamePad {
	[RequireComponent(typeof(Selectable))]
	public class UIPointerEnterSelect : MonoBehaviour, IPointerEnterHandler {
		public void OnPointerEnter(PointerEventData eventData) {
			EventSystem.current.SetSelectedGameObject(gameObject);
		}
	}
}
