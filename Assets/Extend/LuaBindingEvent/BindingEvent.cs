using System;
using Extend.Common;
using Extend.EventAsset;
using Extend.LuaUtil;
using XLua;

namespace Extend.LuaBindingEvent {
	[Serializable]
	public class LuaEmmyFunction {
		public LuaBinding Binding;
		public string LuaMethodName;
		public bool GlobalMethod;

		private Action<LuaTable> m_cachedLuaFunction;

		private T GetLuaMethod<T>() where T : Delegate {
			if( GlobalMethod ) {
				var luaVM = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
				return luaVM.Global.GetInPath<T>(LuaMethodName);
			}
			return Binding.GetLuaMethod<T>(LuaMethodName);
		}

		public void Invoke(EventParam param, object extraData) {
			if( !Binding && !GlobalMethod ) {
				throw new Exception("Event lua binding is null");
			}

			if( string.IsNullOrEmpty(LuaMethodName) ) {
				throw new Exception($"Binding {Binding.name} method is null");
			}

			if( GlobalMethod ) {
				switch( param.Type ) {
					case EventParam.ParamType.None:
						var funcNone = GetLuaMethod<GlobalNoneEventAction>();
						funcNone(extraData);
						break;
					case EventParam.ParamType.Int:
						var funcInt = GetLuaMethod<GlobalIntEventAction>();
						funcInt(extraData, param.Int);
						break;
					case EventParam.ParamType.Float:
						var funcFloat = GetLuaMethod<GlobalFloatEventAction>();
						funcFloat(extraData, param.Float);
						break;
					case EventParam.ParamType.String:
						var funcStr = GetLuaMethod<GlobalStringEventAction>();
						funcStr(extraData, param.Str);
						break;
					case EventParam.ParamType.AssetRef:
						var funcAsset = GetLuaMethod<GlobalAssetEventAction>();
						funcAsset(extraData, param.AssetRef);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			else {
				switch( param.Type ) {
					case EventParam.ParamType.None:
						var funcNone = GetLuaMethod<NoneEventAction>();
						funcNone(Binding.LuaInstance, extraData);
						break;
					case EventParam.ParamType.Int:
						var funcInt = GetLuaMethod<IntEventAction>();
						funcInt(Binding.LuaInstance, extraData, param.Int);
						break;
					case EventParam.ParamType.Float:
						var funcFloat = GetLuaMethod<FloatEventAction>();
						funcFloat(Binding.LuaInstance, extraData, param.Float);
						break;
					case EventParam.ParamType.String:
						var funcStr = GetLuaMethod<StringEventAction>();
						funcStr(Binding.LuaInstance, extraData, param.Str);
						break;
					case EventParam.ParamType.AssetRef:
						var funcAsset = GetLuaMethod<AssetEventAction>();
						funcAsset(Binding.LuaInstance, extraData, param.AssetRef);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		public void Invoke(EventInstance instance) {
			if( m_cachedLuaFunction == null ) {
				if( !Binding && !GlobalMethod ) {
					throw new Exception("Event lua binding is null");
				}

				if( string.IsNullOrEmpty(LuaMethodName) ) {
					throw new Exception($"Binding {Binding.name} method is null");
				}

				if( GlobalMethod ) {
					var luaVM = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
					using var func = luaVM.Global.GetInPath<LuaFunction>(LuaMethodName);
					func.Call(instance);
					return;
				}
				else {
					if( Binding.LuaInstance == null ) {
						throw new Exception($"Binding {Binding.name} lua table instance is null");
					}

					m_cachedLuaFunction = Binding.GetLuaMethod<Action<LuaTable>>(LuaMethodName);
					if( m_cachedLuaFunction == null ) {
						throw new Exception($"Can not find method {LuaMethodName}, Binding : {Binding.name}");
					}
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
