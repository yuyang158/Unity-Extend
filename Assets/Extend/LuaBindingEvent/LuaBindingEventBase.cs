using System;
using System.Collections.Generic;
using Extend.Common;
using Extend.LuaUtil;
using UnityEngine;
using UnityEngine.EventSystems;
using XLua;

namespace Extend.LuaBindingEvent {
	[LuaCallCSharp]
	public abstract class LuaBindingEventBase : MonoBehaviour {
		private static BindingEventDispatch m_dispatch;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
		private static void OnLoad() {
			LuaVM.OnPreRequestLoaded += () => {
				var luaVm = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
				var getLuaService = luaVm.Global.GetInPath<GetLuaService>("_ServiceManager.GetService");
				var eventBindingService = getLuaService(8);
				m_dispatch = eventBindingService.GetInPath<BindingEventDispatch>("Dispatch");
			};
		}
		
		protected void TriggerPointerEvent(string eventName,  IEnumerable<BindingEvent> events, PointerEventData data) {
			foreach( var evt in events ) {
				var emmyFunction = evt.Function;
				switch( evt.Param.Type ) {
					case EventParam.ParamType.None:
						var funcNone = emmyFunction.Binding.LuaInstance.GetInPath<NoneEventAction>(emmyFunction.LuaMethodName);
						funcNone(emmyFunction.Binding.LuaInstance, data);
						break;
					case EventParam.ParamType.Int:
						var funcInt = emmyFunction.Binding.LuaInstance.GetInPath<IntEventAction>(emmyFunction.LuaMethodName);
						funcInt(emmyFunction.Binding.LuaInstance, data, evt.Param.Int);
						break;
					case EventParam.ParamType.Float:
						var funcFloat = emmyFunction.Binding.LuaInstance.GetInPath<FloatEventAction>(emmyFunction.LuaMethodName);
						funcFloat(emmyFunction.Binding.LuaInstance, data, evt.Param.Float);
						break;
					case EventParam.ParamType.String:
						var funcStr = emmyFunction.Binding.LuaInstance.GetInPath<StringEventAction>(emmyFunction.LuaMethodName);
						funcStr(emmyFunction.Binding.LuaInstance, data, evt.Param.Str);
						break;
					case EventParam.ParamType.AssetRef:
						var funcAsset = emmyFunction.Binding.LuaInstance.GetInPath<AssetEventAction>(emmyFunction.LuaMethodName);
						funcAsset(emmyFunction.Binding.LuaInstance, data, evt.Param.AssetRef);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			if( m_luaEvents == null || !m_luaEvents.TryGetValue(eventName, out var eventIds) ) {
				return;
			}

			foreach( var id in eventIds ) {
				m_dispatch(id);
			}
		}

		private Dictionary<string, List<int>> m_luaEvents;
		public void AddEventListener(string eventName, int id) {
			if( m_luaEvents == null ) {
				m_luaEvents = new Dictionary<string, List<int>>();
			}

			if( !m_luaEvents.TryGetValue(eventName, out var ids) ) {
				ids = new List<int> {id};
				m_luaEvents.Add(eventName, ids);
			}
			else {
				ids.Add(id);
			}
		}

		public void RemoveEventListener(string eventName, int id) {
			if( m_luaEvents == null || !m_luaEvents.TryGetValue(eventName, out var eventIds) ) {
				return;
			}

			eventIds.Remove(id);
		}
	}
}