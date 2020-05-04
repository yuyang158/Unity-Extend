using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using XLua;

namespace Extend.LuaBindingEvent {
	public abstract class LuaBindingEventBase : MonoBehaviour {
		protected static void TriggerPointerEvent(IEnumerable<BindingEvent> events, PointerEventData data) {
			foreach( var evt in events ) {
				var emmyFunction = evt.Function;
				var func = emmyFunction.Binding.LuaInstance.GetInPath<LuaFunction>(emmyFunction.LuaMethodName);
				switch( evt.Param.Type ) {
					case EventParam.ParamType.None:
						func.Call(emmyFunction.Binding.LuaInstance, data);
						break;
					case EventParam.ParamType.Int:
						func.Call(emmyFunction.Binding.LuaInstance, data, evt.Param.Int);
						break;
					case EventParam.ParamType.Float:
						func.Call(emmyFunction.Binding.LuaInstance, data, evt.Param.Float);
						break;
					case EventParam.ParamType.String:
						func.Call(emmyFunction.Binding.LuaInstance, data, evt.Param.Str);
						break;
					case EventParam.ParamType.AssetRef:
						func.Call(emmyFunction.Binding.LuaInstance, data, evt.Param.AssetRef);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}
	}
}