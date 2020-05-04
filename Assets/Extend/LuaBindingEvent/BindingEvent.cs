using System;
using Extend.Asset;

namespace Extend.LuaBindingEvent {
	[Serializable]
	public class EventParam {
		public enum ParamType {
			None,
			Int,
			Float,
			String,
			AssetRef
		}

		public int Int;
		public float Float;
		public string Str;
		public AssetReference AssetRef;
		public ParamType Type;
	}

	[Serializable]
	public class LuaEmmyFunction {
		public LuaBinding Binding;
		public string LuaMethodName;
	}
	
	[Serializable]
	public class BindingEvent {
		public LuaEmmyFunction Function;
		public EventParam Param;
	}
}