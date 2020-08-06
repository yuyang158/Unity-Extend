using UnityEngine;
using XLua;

namespace Extend.LuaBindingEvent.AnimationEvent {
	[LuaCallCSharp]
	public class TriggerEventReceiver : EventCallbackReceiver {
		[LuaCallCSharp]
		public class ColliderTriggerEvent : EventInstance {
			public Collider Other;
		}

		private ColliderTriggerEvent m_enterEvent;
		private ColliderTriggerEvent m_leaveEvent;

		private void Awake() {
			m_enterEvent = ScriptableObject.CreateInstance<ColliderTriggerEvent>();
			m_enterEvent.EventName = "enter";
			m_leaveEvent = ScriptableObject.CreateInstance<ColliderTriggerEvent>();
			m_leaveEvent.EventName = "leave";
		}

		private void OnTriggerEnter(Collider other) {
			m_enterEvent.Other = other;
			OnEvent(m_enterEvent);
		}

		private void OnTriggerExit(Collider other) {
			m_leaveEvent.Other = other;
			OnEvent(m_leaveEvent);
		}
	}
}