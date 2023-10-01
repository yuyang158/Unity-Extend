#include "emmy_debugger/api/lua_state.h"
#include "emmy_debugger/lua_version.h"

#define LuaSwitchDo(__LuaJIT__,__Lua51__, __Lua52__ , __Lua53__, __Lua54__,__Default__) return __Lua54__

lua_State* GetMainState(lua_State* L)
{
	LuaSwitchDo(
		GetMainState_luaJIT(L),
		GetMainState_lua51(L),
		GetMainState_lua52(L),
		GetMainState_lua53(L),
		GetMainState_lua54(L),
		nullptr
	);
}

std::vector<lua_State*> FindAllCoroutine(lua_State* L)
{
	LuaSwitchDo(
		std::vector<lua_State*>(),
		FindAllCoroutine_lua51(L),
		FindAllCoroutine_lua52(L),
		FindAllCoroutine_lua53(L),
		FindAllCoroutine_lua54(L),
		std::vector<lua_State*>()
	);
}