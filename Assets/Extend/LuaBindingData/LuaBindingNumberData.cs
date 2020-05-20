using System;
using Extend.Common.Lua;

namespace Extend.LuaBindingData {
	[Serializable]
	public class LuaBindingNumberData : LuaBindingDataBase {
		public double Data;
		public override void ApplyToLuaInstance(ILuaTable instance) {
			instance.Set(FieldName, Data);
		}
	}
}