using System;
using XLua;

namespace Extend.LuaBindingData {
	[Serializable]
	public class LuaBindingIntegerData : LuaBindingDataBase {
		public int Data;
		public override void ApplyToLuaInstance(LuaTable instance) {
			instance.Set(FieldName, Data);
		}
	}
}