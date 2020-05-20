using System;
using Extend.Common.Lua;

namespace Extend.LuaBindingData {
	[Serializable]
	public class LuaBindingStringData : LuaBindingDataBase {
		public string Data;
		public override void ApplyToLuaInstance(ILuaTable instance) {
			instance.Set(FieldName, Data);
		}
	}
}