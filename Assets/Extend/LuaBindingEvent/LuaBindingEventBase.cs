using System;
using System.Collections.Generic;
using Extend.Common;
using Extend.EventAsset;
using Extend.LuaUtil;
using UnityEngine;
using UnityEngine.UI;
using XLua;

namespace Extend.LuaBindingEvent {
	[LuaCallCSharp]
	public abstract class LuaBindingEventBase : MonoBehaviour {
		private static BindingEventDispatch m_dispatch;
		private static BindEvent m_bindEvent;
		public static BindEvent BindEvent => m_bindEvent;
		private static BindEvent m_unbindEvent;
		public static BindEvent UnbindEvent => m_unbindEvent;

		private Selectable m_selectable;
		private bool m_loopEvent;

		protected virtual void Awake() {
			m_selectable = GetComponent<Selectable>();
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
		private static void OnLoad() {
			LuaVM.OnPreRequestLoaded += () => {
				var luaVm = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
				var getLuaService = luaVm.Global.GetInPath<GetLuaService>("_ServiceManager.GetService");
				var eventBindingService = getLuaService(8);
				m_dispatch = eventBindingService.GetInPath<BindingEventDispatch>("Dispatch");
				m_bindEvent = eventBindingService.GetInPath<BindEvent>("AddEventListener");
				m_unbindEvent = eventBindingService.GetInPath<BindEvent>("RemoveEventListener");
			};

			LuaVM.OnVMQuiting += () => {
				m_bindEvent = null;
				m_unbindEvent = null;
				m_dispatch = null;
			};
		}

		protected void TriggerPointerEvent(string eventName, IEnumerable<BindingEvent> events, object data) {
			if( m_selectable != null && !m_selectable.interactable )
				return;

			foreach( var evt in events ) {
				var emmyFunction = evt.Function;
				if( string.IsNullOrEmpty(emmyFunction.LuaMethodName) ) {
					Debug.LogError($"Lua method is empty", this);
					return;
				}
				switch( evt.Param.Type ) {
					case EventParam.ParamType.None:
						var funcNone = emmyFunction.Binding.GetLuaMethod<NoneEventAction>(emmyFunction.LuaMethodName);
						funcNone(emmyFunction.Binding.LuaInstance, data);
						break;
					case EventParam.ParamType.Int:
						var funcInt = emmyFunction.Binding.GetLuaMethod<IntEventAction>(emmyFunction.LuaMethodName);
						funcInt(emmyFunction.Binding.LuaInstance, data, evt.Param.Int);
						break;
					case EventParam.ParamType.Float:
						var funcFloat = emmyFunction.Binding.GetLuaMethod<FloatEventAction>(emmyFunction.LuaMethodName);
						funcFloat(emmyFunction.Binding.LuaInstance, data, evt.Param.Float);
						break;
					case EventParam.ParamType.String:
						var funcStr = emmyFunction.Binding.GetLuaMethod<StringEventAction>(emmyFunction.LuaMethodName);
						funcStr(emmyFunction.Binding.LuaInstance, data, evt.Param.Str);
						break;
					case EventParam.ParamType.AssetRef:
						var funcAsset = emmyFunction.Binding.GetLuaMethod<AssetEventAction>(emmyFunction.LuaMethodName);
						funcAsset(emmyFunction.Binding.LuaInstance, data, evt.Param.AssetRef);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			if( m_eventCache is {Count: > 0} ) {
				m_luaEvents ??= new Dictionary<string, List<int>>();

				foreach( Tuple<string,int> tuple in m_eventCache ) {
					if( !m_luaEvents.TryGetValue(tuple.Item1, out var ids) ) {
						ids = new List<int> {tuple.Item2};
						m_luaEvents.Add(tuple.Item1, ids);
					}
					else {
						ids.Add(tuple.Item2);
					}
				}
				m_eventCache.Clear();
			}

			if( m_luaEvents == null || !m_luaEvents.TryGetValue(eventName, out var eventIds) ) {
				return;
			}

			for( int i = 0; i < eventIds.Count; ) {
				var id = eventIds[i];
				if( id == 0 ) {
					eventIds.RemoveAt(i);
				}
				else {
					m_dispatch(id, data);
					i++;
				}
			}
		}

		private Dictionary<string, List<int>> m_luaEvents;
		private List<Tuple<string, int>> m_eventCache;

		public void AddEventListener(string eventName, int id) {
			m_eventCache ??= new List<Tuple<string, int>>();
			m_eventCache.Add(new Tuple<string, int>(eventName, id));
		}

		public void RemoveEventListener(string eventName, int id) {
			if( m_eventCache is {Count: > 0} ) {
				for( int i = 0; i < m_eventCache.Count; i++ ) {
					var tuple = m_eventCache[i];
					if( tuple.Item2 == id ) {
						m_eventCache.RemoveSwapAt(i);
						break;
					}
				}
			}
			
			
			if( m_luaEvents == null || !m_luaEvents.TryGetValue(eventName, out var eventIds) ) {
				return;
			}

			var index = eventIds.IndexOf(id);
			if( index >= 0 ) {
				eventIds[index] = 0;
			}
		}
	}
}
