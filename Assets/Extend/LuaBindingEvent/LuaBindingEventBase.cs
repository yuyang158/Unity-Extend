using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using XLua;

namespace Extend.LuaBindingEvent {
	public abstract class LuaBindingEventBase : MonoBehaviour {
		protected static void TriggerPointerEvent(IEnumerable<LuaBindingEventData> events, PointerEventData data) {
			foreach( var evt in events ) {
				var func = evt.Binding.LuaInstance.GetInPath<LuaFunction>(evt.LuaMethodName);
				switch( evt.Param.Type ) {
					case LuaBindingEventData.EventParam.ParamType.Int:
						func.Call(evt.Binding.LuaInstance, data, evt.Param.Int);
						break;
					case LuaBindingEventData.EventParam.ParamType.Float:
						func.Call(evt.Binding.LuaInstance, data, evt.Param.Float);
						break;
					case LuaBindingEventData.EventParam.ParamType.String:
						func.Call(evt.Binding.LuaInstance, data, evt.Param.Str);
						break;
					case LuaBindingEventData.EventParam.ParamType.None:
						func.Call(evt.Binding.LuaInstance, data);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}
	}
}