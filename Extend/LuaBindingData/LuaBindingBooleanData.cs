using System;
using XLua;

namespace Extend.LuaBindingData {
	[Serializable]
	public class LuaBindingBooleanData : LuaBindingDataBase {
		public bool Data;
		public override void ApplyToLuaInstance(LuaTable instance) {
			instance.Set(FieldName, Data);
		}
	}
}