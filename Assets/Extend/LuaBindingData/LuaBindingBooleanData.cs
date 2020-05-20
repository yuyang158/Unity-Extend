using System;
using Extend.Common.Lua;

namespace Extend.LuaBindingData {
	[Serializable]
	public class LuaBindingBooleanData : LuaBindingDataBase {
		public bool Data;
		public override void ApplyToLuaInstance(ILuaTable instance) {
			instance.Set(FieldName, Data);
		}
	}
}