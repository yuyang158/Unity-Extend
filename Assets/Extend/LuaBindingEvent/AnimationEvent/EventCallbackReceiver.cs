﻿using System;
using Extend.Common;
using UnityEngine;

namespace Extend.LuaBindingEvent.AnimationEvent {
	public class EventCallbackReceiver : MonoBehaviour {
		[Serializable]
		private class EventInstanceEmmyFunction {
			public EventInstance Event;
			public LuaEmmyFunction Function;
		}

		[SerializeField, ReorderList]
		private EventInstanceEmmyFunction[] Callbacks; 
		
		public void OnEvent(EventInstance instance) {
			foreach( var callback in Callbacks ) {
				if(callback.Event != instance)
					continue;
				
				callback.Function.Invoke(callback.Event);
			}
		}
	}
}