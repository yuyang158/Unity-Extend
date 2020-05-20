using System;
using Extend.Common.Lua;

namespace Extend.LuaBindingData {
	[Serializable]
	public class LuaBindingIntegerData : LuaBindingDataBase {
		public int Data;
		public override void ApplyToLuaInstance(ILuaTable instance) {
			instance.Set(FieldName, Data);
		}
	}
}