using System;
using System.Collections.Generic;
using Extend.Common;
using Extend.LuaUtil;
using UnityEngine;
using XLua;

namespace Extend.LuaBindingEvent.AnimationEvent {
	[LuaCallCSharp]
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
			
			if(m_luaCallbacks == null)
				return;

			foreach( var luaCallback in m_luaCallbacks ) {
				luaCallback(instance);
			}
		}

		private List<GlobalEventCallback> m_luaCallbacks;
		public void AddLuaCallback(GlobalEventCallback callback) {
			if( m_luaCallbacks == null ) {
				m_luaCallbacks = new List<GlobalEventCallback>();
			}
			m_luaCallbacks.Add(callback);
		}

		public void RemoveLuaCallback(GlobalEventCallback callback) {
			m_luaCallbacks.Remove(callback);
		}
	}
}