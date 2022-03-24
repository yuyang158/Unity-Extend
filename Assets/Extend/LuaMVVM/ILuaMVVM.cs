using XLua;

namespace Extend.LuaMVVM {
	[LuaCallCSharp]
	public interface ILuaMVVM : IMVVMDetach {
		void SetDataContext(LuaTable dataSource);
		LuaTable GetDataContext();
	}
}