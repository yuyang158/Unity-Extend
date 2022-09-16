using System.Collections.Generic;
using Extend.LuaUtil;
using UnityEngine;
using XLua;

namespace Extend.LuaBindingEvent {
	[CSharpCallLua, LuaCallCSharp]
	public class LuaGlobalEvent : MonoBehaviour {
		private static readonly Dictionary<string, LuaGlobalEventCallback> m_eventCallbacks = new Dictionary<string, LuaGlobalEventCallback>();
		public static void Register(string e, LuaGlobalEventCallback callback) {
			m_eventCallbacks.Add(e, callback);
		}

		private static void Trigger(string eventName, string eventContent) {
			if(!m_eventCallbacks.TryGetValue(eventName, out var cb)) {
				return;
			}

			cb.Invoke(eventContent);
		}

		public string EventName;
		public string EventContent;

		[BlackList]
		public void Dispatch() {
			Trigger(EventName, EventContent);
		}
	}
}