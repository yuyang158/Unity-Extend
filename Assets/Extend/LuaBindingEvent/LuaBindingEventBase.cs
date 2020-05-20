using System;
using System.Collections.Generic;
using Extend.Asset;
using UnityEngine;
using UnityEngine.EventSystems;
using XLua;

namespace Extend.LuaBindingEvent {
	public abstract class LuaBindingEventBase : MonoBehaviour {
		[CSharpCallLua]
		private delegate void NoneEventAction(LuaTable t, PointerEventData data);
		[CSharpCallLua]
		private delegate void IntEventAction(LuaTable t, PointerEventData data, int val);
		[CSharpCallLua]
		private delegate void FloatEventAction(LuaTable t, PointerEventData data, float val);
		[CSharpCallLua]
		private delegate void StringEventAction(LuaTable t, PointerEventData data, string val);
		[CSharpCallLua]
		private delegate void AssetEventAction(LuaTable t, PointerEventData data, AssetReference val);
		
		protected static void TriggerPointerEvent(IEnumerable<BindingEvent> events, PointerEventData data) {
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
		}
	}
}