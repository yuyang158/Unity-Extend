using System;
using UnityEngine;

namespace Extend.LuaBindingEvent.AnimationEvent {
	public class EventCallbackBehaviour : MonoBehaviour {
		[Serializable]
		private class EventInstanceEmmyFunction {
			public EventInstance Event;
			public LuaEmmyFunction Function;
		}

		[SerializeField]
		private EventInstanceEmmyFunction[] Callbacks; 
		
		public void OnEvent(EventInstance instance) {
		}
	}
}