using System;
using XLua;

namespace Extend.LuaBindingData {
	[Serializable, UnityEngine.Scripting.Preserve]
	public class LuaBindingBooleanData : LuaBindingDataBase {
		public bool Data;
		public override void ApplyToLuaInstance(LuaTable instance) {
			instance.Set(FieldName, Data);
		}
	}
}