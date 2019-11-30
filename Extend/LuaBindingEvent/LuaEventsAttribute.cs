using UnityEngine;

namespace Extend.LuaBindingEvent {
	public class LuaEventsAttribute : PropertyAttribute {
		public readonly string EvtName;
		public LuaEventsAttribute( string evtName ) {
			EvtName = evtName;
		}
	}
}