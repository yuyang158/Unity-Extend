using Extend.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Extend.LuaBindingEvent {
	[RequireComponent(typeof(Button))]
	public class LuaBindingClickEvent : LuaBindingEventBase {
		[ReorderList, LabelText("On Click ()"), SerializeField]
		private BindingEvent[] m_clickEvent;

		private void Start() {
			var button = GetComponent<Button>();
			button.onClick.AddListener(OnPointerClick);
		}

		public void OnPointerClick() {
			TriggerPointerEvent("OnClick", m_clickEvent, null);
		}
	}
} 