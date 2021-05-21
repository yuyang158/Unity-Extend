using UnityEngine;
using UnityEngine.EventSystems;

namespace Extend.LuaBindingEvent.RoutedEvent {
	public abstract class UpwardEventBase : MonoBehaviour {
		private LuaBindingUpwardEventReceiver m_receiver;
		private void OnTransformParentChanged() {
			if( transform.parent == null ) {
				m_receiver = null;
				return;
			}

			m_receiver = GetComponentInParent<LuaBindingUpwardEventReceiver>();
		}

		protected void RouteEvent(string eventName, PointerEventData eventData) {
			m_receiver.OnEvent(eventName, eventData);
		}
	}
}