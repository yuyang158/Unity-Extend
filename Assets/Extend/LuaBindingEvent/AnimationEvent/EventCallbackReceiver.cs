using System;
using System.Collections.Generic;
using Extend.Common;
using Extend.EventAsset;
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
		private readonly List<LuaEventCallback> m_removed = new List<LuaEventCallback>();
		private bool m_dispatching;

		public void OnEvent(EventInstance instance) {
			foreach( var callback in Callbacks ) {
				if(callback.Event != instance)
					continue;
				
				callback.Function.Invoke(callback.Event);
			}
			
			if(m_luaCallbacks == null)
				return;

			m_dispatching = true;
			m_removed.Clear();
			var count = m_luaCallbacks.Count;
			for( var i = 0; i < count; i++ ) {
				var luaCallback = m_luaCallbacks[i];
				if( luaCallback(instance) ) {
					m_removed.Add(luaCallback);
				}
			}

			m_dispatching = false;
			foreach( var remove in m_removed ) {
				m_luaCallbacks.Remove(remove);
			}
		}

		private List<LuaEventCallback> m_luaCallbacks;
		public void AddLuaCallback(LuaEventCallback callback) {
			if( m_luaCallbacks == null ) {
				m_luaCallbacks = new List<LuaEventCallback>();
			}
			m_luaCallbacks.Add(callback);
		}

		public void RemoveLuaCallback(LuaEventCallback callback) {
			if( m_dispatching ) {
				m_removed.Add(callback);
			}
			else {
				m_luaCallbacks.Remove(callback);
			}
		}
	}
}