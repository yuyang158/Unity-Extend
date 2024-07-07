using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Extend.UI.GamePad {
	[RequireComponent(typeof(Toggle))]
	public class UIToggleGamepadSelect : MonoBehaviour, ISelectHandler {
		private Toggle m_toggle;

		private void Awake() {
			m_toggle = GetComponent<Toggle>();
		}

		public void OnSelect(BaseEventData eventData) {
			m_toggle.isOn = true;
		}
	}
}