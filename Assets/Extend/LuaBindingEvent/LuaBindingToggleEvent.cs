using Extend.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Extend.LuaBindingEvent {
	[RequireComponent(typeof(Toggle))]
	public class LuaBindingToggleEvent : LuaBindingEventBase {
		[ReorderList, LabelText("On Check ()"), SerializeField]
		private BindingEvent[] m_checkEvent;

		private void Start() {
			var toggle = GetComponent<Toggle>();
			toggle.onValueChanged.AddListener(OnValueChanged);
		}

		public void OnValueChanged(bool isOn) {
			TriggerPointerEvent("OnCheck", m_checkEvent, isOn);
		}
	}
}