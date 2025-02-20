using System;
using UnityEngine;
using XLua;

namespace Extend.LuaBindingData {
	[Serializable, UnityEngine.Scripting.Preserve]
	public class LuaBindingIntegerArrayData : LuaBindingDataBase {
		public int[] Data;
		public override void ApplyToLuaInstance(LuaTable instance) {
			var tbl = instance.LuaEnv.NewTable();
			for( int i = 0; i < Data.Length; i++ ) {
				tbl.Set(i + 1, Data[i]);
			}
			instance.Set(FieldName, tbl);
		}
	}
}