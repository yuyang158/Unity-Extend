using System;
using XLua;

namespace Extend.LuaBindingData {
	[Serializable, UnityEngine.Scripting.Preserve]
	public class LuaBindingNumberData : LuaBindingDataBase {
		public double Data;
		public override void ApplyToLuaInstance(LuaTable instance) {
			instance.Set(FieldName, Data);
		}
	}
}