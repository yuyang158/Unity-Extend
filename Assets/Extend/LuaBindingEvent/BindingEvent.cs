using System;
using Extend.Asset;
using Extend.LuaBindingEvent.AnimationEvent;
using XLua;

namespace Extend.LuaBindingEvent {
	[Serializable]
	public class EventParam {
		public enum ParamType : byte {
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

		private Action<LuaTable> m_cachedLuaFunction;

		public void Invoke(EventInstance instance) {
			if( m_cachedLuaFunction == null ) {
				if( !Binding ) {
					throw new Exception("Event lua binding is null");
				}

				if( Binding.LuaInstance == null ) {
					throw new Exception($"Binding {Binding.name} lua table instance is null");
				}

				if( string.IsNullOrEmpty(LuaMethodName) ) {
					throw new Exception($"Binding {Binding.name} method is null");
				}

				m_cachedLuaFunction = Binding.LuaInstance.Get<Action<LuaTable>>(LuaMethodName);
				if( m_cachedLuaFunction == null ) {
					throw new Exception($"Can not find method {LuaMethodName}, Binding : {Binding.name}");
				}
			}

			m_cachedLuaFunction(Binding.LuaInstance);
		}
	}

	[Serializable]
	public class BindingEvent {
		public LuaEmmyFunction Function;
		public EventParam Param;
	}
}