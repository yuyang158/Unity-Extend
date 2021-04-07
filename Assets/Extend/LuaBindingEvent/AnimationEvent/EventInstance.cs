using System;
using Extend.Asset;
using UnityEngine;
using XLua;

namespace Extend.LuaBindingEvent.AnimationEvent {
	[CreateAssetMenu(fileName = "EventInstance", menuName = "Event/Event Instance", order = 1), LuaCallCSharp]
	public class EventInstance : ScriptableObject {
		public string EventName;
		public EventParam Param;

		public object Value {
			get {
				switch( Param.Type ) {
					case EventParam.ParamType.None:
						return null;
					case EventParam.ParamType.Int:
						return Param.Int;
					case EventParam.ParamType.Float:
						return Param.Float;
					case EventParam.ParamType.String:
						return Param.Str;
					case EventParam.ParamType.AssetRef:
						return Param.AssetRef.Clone();
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}
	}
}