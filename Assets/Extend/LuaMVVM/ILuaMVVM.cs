using XLua;

namespace Extend.LuaMVVM {
	public interface ILuaMVVM {
		void SetDataContext(LuaTable dataSource);

		LuaTable GetDataContext();

		void Detach();
	}
}