using System.Collections.Generic;
using System.Diagnostics;
using Extend.LuaUtil;
using XLua;

namespace Extend.LuaBindingEvent {
	[CSharpCallLua]
	public static class LuaGlobalEvent {
		private static Dictionary<EventInstance, GlobalEventCallback> m_eventCallbacks = new Dictionary<EventInstance, GlobalEventCallback>();
		public static void Register(EventInstance e, GlobalEventCallback callback) {
			m_eventCallbacks.Add(e, callback);
		}

		public static void Trigger(EventInstance e) {
			if(!m_eventCallbacks.TryGetValue(e, out var cb)) {
				return;
			}

			cb.Invoke();
		} 
	}
}