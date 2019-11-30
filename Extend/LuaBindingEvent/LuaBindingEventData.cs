using System;

namespace Extend.LuaBindingEvent {
	[Serializable]
	public class LuaBindingEventData {
		[Serializable]
		public struct EventParam {
			public enum ParamType {
				Int,
				Float,
				String,
				None
			}

			public int Int;
			public float Float;
			public string Str;
			public ParamType Type;
		}
		
		public LuaBinding Binding;
		public EventParam Param;
		public string LuaMethodName;
	}

	[Serializable]
	public class LuaBindingEvents {
		public LuaBindingEventData[] Events;
	}
}