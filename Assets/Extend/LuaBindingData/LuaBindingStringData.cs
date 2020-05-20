using System;
using XLua;

namespace Extend.LuaBindingData {
	[Serializable]
	public class LuaBindingStringData : LuaBindingDataBase {
		public string Data;
		public override void ApplyToLuaInstance(LuaTable instance) {
			instance.Set(FieldName, Data);
		}
	}
}