using XLua;

namespace Extend.LuaMVVM {
	public interface ILuaMVVM : IMVVMDetach {
		void SetDataContext(LuaTable dataSource);
		LuaTable GetDataContext();
	}
}