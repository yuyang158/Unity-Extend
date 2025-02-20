using Extend.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Extend.LuaBindingEvent {
	[RequireComponent(typeof(Button))]
	public class LuaBindingClickEvent : LuaBindingEventBase {
		[ReorderList, LabelText("On Click ()"), SerializeField]
		private BindingEvent[] m_clickEvent;

		private Button m_button;
		private void Start() {
			m_button = GetComponent<Button>();
			m_button.onClick.AddListener(OnPointerClick);
		}

		public void OnPointerClick() {
			TriggerPointerEvent("OnClick", m_clickEvent, m_button);
		}
	}
} 