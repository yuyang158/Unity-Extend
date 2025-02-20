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

		[SerializeField]
		private bool m_ignoreInteractable;

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
			if( !m_ignoreInteractable ) {
				if( m_selectable != null && !m_selectable.IsInteractable() )
					return;
			}

			foreach( var evt in events ) {
				var emmyFunction = evt.Function;
				if( string.IsNullOrEmpty(emmyFunction.LuaMethodName) ) {
					Debug.LogError($"Lua method is empty", this);
					return;
				}
				emmyFunction.Invoke(evt.Param, data);
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
