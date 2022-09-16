using System;
using System.Collections.Generic;
using Extend.LuaBindingEvent;

namespace Extend.LuaMVVM.Editor {
	public static class LuaBindingEventNameConstant {
		private static Dictionary<Type, string[]> m_event2Names = new Dictionary<Type, string[]>() {
			{typeof(LuaBindingClickEvent), new[] {"OnClick"}},
			{typeof(LuaBindingDragEvent), new[] {"OnBeginDrag", "OnDrag", "OnEndDrag"}},
			{typeof(LuaBindingUpDownMoveEvent), new[] {"OnDown", "OnUp", "OnDrag"}}
		};
	}
}