/*
 *Tencent is pleased to support the open source community by making xLua available.
 *Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 *Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 *http://opensource.org/licenses/MIT
 *Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

#define LUA_LIB

#include "lua.h"
#include "lualib.h"
#include "lauxlib.h"

#include <string.h>
#include <stdint.h>
#include "i64lib.h"

#if USING_LUAJIT
#include "lj_obj.h"
#else
#include "lstate.h"
#endif

/*
** stdcall C function support
*/

static int tag = 0;
static const char *const hooknames[] = {"call", "return", "line", "count", "tail return"};
static int hook_index = -1;

LUA_API void *xmoon_tag () 
{
	return &tag;
}

LUA_API int xmoon_get_registry_index() {
	return LUA_REGISTRYINDEX;
}

LUA_API int xmoon_get_lib_version() {
	return 105;
}

LUA_API int xmoon_tocsobj_safe(lua_State *L,int index) {
	int *udata = (int *)moon_touserdata (L,index);
	if (udata != NULL) {
		if (moon_getmetatable(L,index)) {
		    moon_pushlightuserdata(L, &tag);
			moon_rawget(L,-2);
			if (!lua_isnil (L,-1)) {
				lua_pop (L, 2);
				return *udata;
			}
			lua_pop (L, 2);
		}
	}
	return -1;
}

LUA_API int xmoon_tocsobj_fast (lua_State *L,int index) {
	int *udata = (int *)moon_touserdata (L,index);

	if(udata!=NULL) 
		return *udata;
	return -1;
}

#if LUA_VERSION_NUM == 501
#undef lua_getglobal
LUA_API int lua_getglobal (lua_State *L, const char *name) {
	lua_getfield(L, LUA_GLOBALSINDEX, name);
	return 0;
}

#undef lua_setglobal
LUA_API void lua_setglobal (lua_State *L, const char *name) {
	lua_setfield(L, LUA_GLOBALSINDEX, name);
}

LUA_API int lua_isinteger (lua_State *L, int idx) {
	return 0;
}

LUA_API uint32_t xmoon_objlen (lua_State *L, int idx) {
	return lua_objlen (L, idx);
}

LUA_API uint32_t xmoon_touint (lua_State *L, int idx) {
	return (uint32_t)lua_tonumber(L, idx);
}

LUA_API void xmoon_pushuint (lua_State *L, uint32_t n) {
	lua_pushnumber(L, n);
}
#endif

#if LUA_VERSION_NUM >= 503
LUA_API int moon_setfenv(lua_State *L, int idx)
{
    int type = moon_type(L, idx);
    if(type == LUA_TUSERDATA || type == LUA_TFUNCTION)
    {
        moon_setupvalue(L, idx, 1);
        return 1;
    }
    else
    {
        return 0;
    }
}

LUA_API uint32_t xmoon_objlen (lua_State *L, int idx) {
	return (uint32_t)moonL_len (L, idx);
}

LUA_API uint32_t xmoon_touint (lua_State *L, int idx) {
	return moon_isinteger(L, idx) ? (uint32_t)lua_tointeger(L, idx) : (uint32_t) moon_tonumber(L, idx);
}

LUA_API void xmoon_pushuint (lua_State *L, uint32_t n) {
	moon_pushinteger(L, n);
}

#undef moon_insert
LUA_API void moon_insert(lua_State *L, int idx) {
    moon_rotate(L, idx, 1);
}

#undef moon_remove
LUA_API void moon_remove(lua_State *L, int idx) {
	moon_rotate(L, idx, -1);
	lua_pop(L, 1);
}

#undef moon_replace
LUA_API void moon_replace(lua_State *L, int idx) {
	moon_copy(L, -1, idx);
	lua_pop(L, 1);
}

#undef moon_pcall
LUA_API int moon_pcall(lua_State *L, int nargs, int nresults, int errfunc) {
	return moon_pcallk(L, nargs, nresults, errfunc, 0, NULL);
}

#undef moon_tonumber
LUA_API lua_Number moon_tonumber(lua_State *L, int idx) {
	return moon_tonumberx(L, idx, NULL);
}

#endif

#if LUA_VERSION_NUM < 503
#define lua_absindex(L, index) ((index > 0 || index <= LUA_REGISTRYINDEX) ? index : lua_gettop(L) + index + 1)
#endif

LUA_API void xmoon_getloaders (lua_State *L) {
	moon_getglobal(L, "package");
#if LUA_VERSION_NUM == 501
    lua_getfield(L, -1, "loaders");
#else
	moon_getfield(L, -1, "searchers");
#endif
	moon_remove(L, -2);
}

LUA_API void xmoon_rawgeti (lua_State *L, int idx, int64_t n) {
	moon_rawgeti(L, idx, (lua_Integer)n);
}

LUA_API void xmoon_rawseti (lua_State *L, int idx, int64_t n) {
	moon_rawseti(L, idx, (lua_Integer)n);
}

LUA_API int xmoon_ref_indirect(lua_State *L, int indirectRef) {
	int ret = 0;
	moon_rawgeti(L, LUA_REGISTRYINDEX, indirectRef);
	moon_pushvalue(L, -2);
	ret = moonL_ref(L, -2);
	lua_pop(L, 2);
	return ret;
}

LUA_API void xmoon_getref_indirect(lua_State *L, int indirectRef, int reference) {
	moon_rawgeti(L, LUA_REGISTRYINDEX, indirectRef);
	moon_rawgeti(L, -1, reference);
	moon_remove(L, -2);
}

LUA_API int xmoon_tointeger (lua_State *L, int idx) {
	return (int)lua_tointeger(L, idx);
}

LUA_API void xmoon_pushinteger (lua_State *L, int n) {
	moon_pushinteger(L, n);
}

LUA_API void xmoon_pushlstring (lua_State *L, const char *s, int len) {
	moon_pushlstring(L, s, len);
}

LUALIB_API int xluaL_loadbuffer (lua_State *L, const char *buff, int size,
                                const char *name) {
	return luaL_loadbuffer(L, buff, size, name);
}

static int c_lua_gettable(lua_State* L) {    
    moon_gettable(L, 1);    
    return 1;
}

LUA_API int xmoon_pgettable(lua_State* L, int idx) {
    int top = moon_gettop(L);
    idx = moon_absindex(L, idx);
    lua_pushcfunction(L, c_lua_gettable);
    moon_pushvalue(L, idx);
    moon_pushvalue(L, top);
    moon_remove(L, top);
    return moon_pcall(L, 2, 1, 0);
}

static int c_lua_gettable_bypath(lua_State* L) {
	size_t len = 0;
	const char * pos = NULL;
	const char * path = moon_tolstring(L, 2, &len);
	moon_pushvalue(L, 1);
	do {
		pos = strchr(path, '.');
		if (NULL == pos) {
			moon_pushlstring(L, path, len);
		} else {
			moon_pushlstring(L, path, pos - path);
			len = len - (pos - path + 1);
			path = pos + 1;
		}
		moon_gettable(L, -2);
		if (moon_type(L, -1) != LUA_TTABLE) {
			if (NULL != pos) { // not found in path
				moon_pushnil(L);
			}
			break;
		}
		moon_remove(L, -2);
	} while(pos);
    return 1;
}

LUA_API int xmoon_pgettable_bypath(lua_State* L, int idx, const char *path) {
	idx = moon_absindex(L, idx);
	lua_pushcfunction(L, c_lua_gettable_bypath);
	moon_pushvalue(L, idx);
	moon_pushstring(L, path);
	return moon_pcall(L, 2, 1, 0);
}

static int c_lua_settable(lua_State* L) {
    moon_settable(L, 1);
    return 0;
}

LUA_API int xmoon_psettable(lua_State* L, int idx) {
    int top = moon_gettop(L);
    idx = moon_absindex(L, idx);
    lua_pushcfunction(L, c_lua_settable);
    moon_pushvalue(L, idx);
    moon_pushvalue(L, top - 1);
    moon_pushvalue(L, top);
    moon_remove(L, top);
    moon_remove(L, top - 1);
    return moon_pcall(L, 3, 0, 0);
}

static int c_lua_settable_bypath(lua_State* L) {
    size_t len = 0;
	const char * pos = NULL;
	const char * path = moon_tolstring(L, 2, &len);
	moon_pushvalue(L, 1);
	do {
		pos = strchr(path, '.');
		if (NULL == pos) { // last
			moon_pushlstring(L, path, len);
			moon_pushvalue(L, 3);
			moon_settable(L, -3);
			lua_pop(L, 1);
			break;
		} else {
			moon_pushlstring(L, path, pos - path);
			len = len - (pos - path + 1);
			path = pos + 1;
		}
		moon_gettable(L, -2);
		if (moon_type(L, -1) != LUA_TTABLE) {
			return moonL_error(L, "can not set value to %s", lua_tostring(L, 2));
		}
		moon_remove(L, -2);
	} while(pos);
    return 0;
}

LUA_API int xmoon_psettable_bypath(lua_State* L, int idx, const char *path) {
    int top = moon_gettop(L);
    idx = moon_absindex(L, idx);
    lua_pushcfunction(L, c_lua_settable_bypath);
    moon_pushvalue(L, idx);
    moon_pushstring(L, path);
    moon_pushvalue(L, top);
    moon_remove(L, top);
    return moon_pcall(L, 3, 0, 0);
}

static int c_lua_getglobal(lua_State* L) {
	moon_getglobal(L, lua_tostring(L, 1));
	return 1;
}

LUA_API int xmoon_getglobal (lua_State *L, const char *name) {
	lua_pushcfunction(L, c_lua_getglobal);
	moon_pushstring(L, name);
	return moon_pcall(L, 1, 1, 0);
}

static int c_lua_setglobal(lua_State* L) {
	moon_setglobal(L, lua_tostring(L, 1));
	return 0;
}

LUA_API int xmoon_setglobal (lua_State *L, const char *name) {
	int top = moon_gettop(L);
	lua_pushcfunction(L, c_lua_setglobal);
	moon_pushstring(L, name);
	moon_pushvalue(L, top);
	moon_remove(L, top);
	return moon_pcall(L, 2, 0, 0);
}

LUA_API int xmoon_tryget_cachedud(lua_State *L, int key, int cache_ref) {
	moon_rawgeti(L, LUA_REGISTRYINDEX, cache_ref);
	moon_rawgeti(L, -1, key);
	if (!lua_isnil(L, -1))
	{
		moon_remove(L, -2);
		return 1;
	}
	lua_pop(L, 2);
	return 0;
}

static void cacheud(lua_State *L, int key, int cache_ref) {
	moon_rawgeti(L, LUA_REGISTRYINDEX, cache_ref);
	moon_pushvalue(L, -2);
	moon_rawseti(L, -2, key);
	lua_pop(L, 1);
}


LUA_API void xmoon_pushcsobj(lua_State *L, int key, int meta_ref, int need_cache, int cache_ref) {
	int* pointer = (int*)lua_newuserdata(L, sizeof(int));
	*pointer = key;
	
	if (need_cache) cacheud(L, key, cache_ref);

    moon_rawgeti(L, LUA_REGISTRYINDEX, meta_ref);

	moon_setmetatable(L, -2);
}

void print_top(lua_State *L) {
	moon_getglobal(L, "print");
	moon_pushvalue(L, -2);
	lua_call(L, 1, 0);
}

void print_str(lua_State *L, char *str) {
	moon_getglobal(L, "print");
	moon_pushstring(L, str);
	lua_call(L, 1, 0);
}

void print_value(lua_State *L,  char *str, int idx) {
	idx = moon_absindex(L, idx);
	moon_getglobal(L, "print");
	moon_pushstring(L, str);
	moon_pushvalue(L, idx);
	lua_call(L, 2, 0);
}

//upvalue --- [1]: methods, [2]:getters, [3]:csindexer, [4]:base, [5]:indexfuncs, [6]:arrayindexer, [7]:baseindex
//param   --- [1]: obj, [2]: key
LUA_API int obj_indexer(lua_State *L) {	
	if (!lua_isnil(L, lua_upvalueindex(1))) {
		moon_pushvalue(L, 2);
		moon_gettable(L, lua_upvalueindex(1));
		if (!lua_isnil(L, -1)) {//has method
			return 1;
		}
		lua_pop(L, 1);
	}
	
	if (!lua_isnil(L, lua_upvalueindex(2))) {
		moon_pushvalue(L, 2);
		moon_gettable(L, lua_upvalueindex(2));
		if (!lua_isnil(L, -1)) {//has getter
			moon_pushvalue(L, 1);
			lua_call(L, 1, 1);
			return 1;
		}
		lua_pop(L, 1);
	}
	
	
	if (!lua_isnil(L, lua_upvalueindex(6)) && moon_type(L, 2) == LUA_TNUMBER) {
		moon_pushvalue(L, lua_upvalueindex(6));
		moon_pushvalue(L, 1);
		moon_pushvalue(L, 2);
		lua_call(L, 2, 1);
		return 1;
	}
	
	if (!lua_isnil(L, lua_upvalueindex(3))) {
		moon_pushvalue(L, lua_upvalueindex(3));
		moon_pushvalue(L, 1);
		moon_pushvalue(L, 2);
		lua_call(L, 2, 2);
		if (moon_toboolean(L, -2)) {
			return 1;
		}
		lua_pop(L, 2);
	}
	
	if (!lua_isnil(L, lua_upvalueindex(4))) {
		moon_pushvalue(L, lua_upvalueindex(4));
		while(!lua_isnil(L, -1)) {
			moon_pushvalue(L, -1);
			moon_gettable(L, lua_upvalueindex(5));
			if (!lua_isnil(L, -1)) // found
			{
                moon_replace(L, lua_upvalueindex(7)); //baseindex = indexfuncs[base]
				lua_pop(L, 1);
				break;
			}
			lua_pop(L, 1);
			moon_getfield(L, -1, "BaseType");
			moon_remove(L, -2);
		}
		moon_pushnil(L);
        moon_replace(L, lua_upvalueindex(4));//base = nil
	}
	
	if (!lua_isnil(L, lua_upvalueindex(7))) {
		moon_settop(L, 2);
		moon_pushvalue(L, lua_upvalueindex(7));
		moon_insert(L, 1);
		lua_call(L, 2, 1);
		return 1;
	} else {
		return 0;
	}
}

LUA_API int gen_obj_indexer(lua_State *L) {
	moon_pushnil(L);
	moon_pushcclosure(L, obj_indexer, 7);
	return 0;
}

//upvalue --- [1]:setters, [2]:csnewindexer, [3]:base, [4]:newindexfuncs, [5]:arrayindexer, [6]:basenewindex
//param   --- [1]: obj, [2]: key, [3]: value
LUA_API int obj_newindexer(lua_State *L) {
	if (!lua_isnil(L, lua_upvalueindex(1))) {
		moon_pushvalue(L, 2);
		moon_gettable(L, lua_upvalueindex(1));
		if (!lua_isnil(L, -1)) {//has setter
			moon_pushvalue(L, 1);
			moon_pushvalue(L, 3);
			lua_call(L, 2, 0);
			return 0;
		}
		lua_pop(L, 1);
	}
	
	if (!lua_isnil(L, lua_upvalueindex(2))) {
		moon_pushvalue(L, lua_upvalueindex(2));
		moon_pushvalue(L, 1);
		moon_pushvalue(L, 2);
		moon_pushvalue(L, 3);
		lua_call(L, 3, 1);
		if (moon_toboolean(L, -1)) {
			return 0;
		}
	}
	
	if (!lua_isnil(L, lua_upvalueindex(5)) && moon_type(L, 2) == LUA_TNUMBER) {
		moon_pushvalue(L, lua_upvalueindex(5));
		moon_pushvalue(L, 1);
		moon_pushvalue(L, 2);
		moon_pushvalue(L, 3);
		lua_call(L, 3, 0);
		return 0;
	}
	
	if (!lua_isnil(L, lua_upvalueindex(3))) {
		moon_pushvalue(L, lua_upvalueindex(3));
		while(!lua_isnil(L, -1)) {
			moon_pushvalue(L, -1);
			moon_gettable(L, lua_upvalueindex(4));

			if (!lua_isnil(L, -1)) // found
			{
                moon_replace(L, lua_upvalueindex(6)); //basenewindex = newindexfuncs[base]
				lua_pop(L, 1);
				break;
			}
			lua_pop(L, 1);
			moon_getfield(L, -1, "BaseType");
			moon_remove(L, -2);
		}
		moon_pushnil(L);
        moon_replace(L, lua_upvalueindex(3));//base = nil
	}
	
	if (!lua_isnil(L, lua_upvalueindex(6))) {
		moon_settop(L, 3);
		moon_pushvalue(L, lua_upvalueindex(6));
		moon_insert(L, 1);
		lua_call(L, 3, 0);
		return 0;
	} else {
		return moonL_error(L, "cannot set %s, no such field", lua_tostring(L, 2));
	}
}

LUA_API int gen_obj_newindexer(lua_State *L) {
	moon_pushnil(L);
	moon_pushcclosure(L, obj_newindexer, 6);
	return 0;
}

//upvalue --- [1]:getters, [2]:feilds, [3]:base, [4]:indexfuncs, [5]:baseindex
//param   --- [1]: obj, [2]: key
LUA_API int cls_indexer(lua_State *L) {	
	if (!lua_isnil(L, lua_upvalueindex(1))) {
		moon_pushvalue(L, 2);
		moon_gettable(L, lua_upvalueindex(1));
		if (!lua_isnil(L, -1)) {//has getter
			lua_call(L, 0, 1);
			return 1;
		}
		lua_pop(L, 1);
	}
	
	if (!lua_isnil(L, lua_upvalueindex(2))) {
		moon_pushvalue(L, 2);
		moon_rawget(L, lua_upvalueindex(2));
		if (!lua_isnil(L, -1)) {//has feild
			return 1;
		}
		lua_pop(L, 1);
	}
	
	if (!lua_isnil(L, lua_upvalueindex(3))) {
		moon_pushvalue(L, lua_upvalueindex(3));
		while(!lua_isnil(L, -1)) {
			moon_pushvalue(L, -1);
			moon_gettable(L, lua_upvalueindex(4));
			if (!lua_isnil(L, -1)) // found
			{
                moon_replace(L, lua_upvalueindex(5)); //baseindex = indexfuncs[base]
				lua_pop(L, 1);
				break;
			}
			lua_pop(L, 1);
			moon_getfield(L, -1, "BaseType");
			moon_remove(L, -2);
		}
		moon_pushnil(L);
		moon_replace(L, lua_upvalueindex(3));//base = nil
	}
	
	if (!lua_isnil(L, lua_upvalueindex(5))) {
		moon_settop(L, 2);
		moon_pushvalue(L, lua_upvalueindex(5));
		moon_insert(L, 1);
		lua_call(L, 2, 1);
		return 1;
	} else {
		moon_pushnil(L);
		return 1;
	}
}

LUA_API int gen_cls_indexer(lua_State *L) {
	moon_pushnil(L);
	moon_pushcclosure(L, cls_indexer, 5);
	return 0;
}

//upvalue --- [1]:setters, [2]:base, [3]:indexfuncs, [4]:baseindex
//param   --- [1]: obj, [2]: key, [3]: value
LUA_API int cls_newindexer(lua_State *L) {	
	if (!lua_isnil(L, lua_upvalueindex(1))) {
		moon_pushvalue(L, 2);
		moon_gettable(L, lua_upvalueindex(1));
		if (!lua_isnil(L, -1)) {//has setter
		    moon_pushvalue(L, 3);
			lua_call(L, 1, 0);
			return 0;
		}
	}
	
	if (!lua_isnil(L, lua_upvalueindex(2))) {
		moon_pushvalue(L, lua_upvalueindex(2));
		while(!lua_isnil(L, -1)) {
			moon_pushvalue(L, -1);
			moon_gettable(L, lua_upvalueindex(3));
			if (!lua_isnil(L, -1)) // found
			{
                moon_replace(L, lua_upvalueindex(4)); //baseindex = indexfuncs[base]
				lua_pop(L, 1);
				break;
			}
			lua_pop(L, 1);
			moon_getfield(L, -1, "BaseType");
			moon_remove(L, -2);
		}
		moon_pushnil(L);
        moon_replace(L, lua_upvalueindex(2));//base = nil
	}
	
	if (!lua_isnil(L, lua_upvalueindex(4))) {
		moon_settop(L, 3);
		moon_pushvalue(L, lua_upvalueindex(4));
		moon_insert(L, 1);
		lua_call(L, 3, 0);
		return 0;
	} else {
		return moonL_error(L, "no static field %s", lua_tostring(L, 2));
	}
}

LUA_API int gen_cls_newindexer(lua_State *L) {
	moon_pushnil(L);
	moon_pushcclosure(L, cls_newindexer, 4);
	return 0;
}

LUA_API int errorfunc(lua_State *L) {
	moon_getglobal(L, "debug");
	moon_getfield(L, -1, "traceback");
	moon_remove(L, -2);
	moon_pushvalue(L, 1);
	moon_pushnumber(L, 2);
	lua_call(L, 2, 1);
    return 1;
}

LUA_API int get_error_func_ref(lua_State *L) {
	moon_pushcclosure(L, errorfunc, 0);
	return moonL_ref(L, LUA_REGISTRYINDEX);
}

LUA_API int load_error_func(lua_State *L, int ref) {
	moon_rawgeti(L, LUA_REGISTRYINDEX, ref);
	return moon_gettop(L);
}

LUA_API int pcall_prepare(lua_State *L, int error_func_ref, int func_ref) {
	moon_rawgeti(L, LUA_REGISTRYINDEX, error_func_ref);
	moon_rawgeti(L, LUA_REGISTRYINDEX, func_ref);
	return moon_gettop(L) - 1;
}

static void hook(lua_State *L, lua_Debug *ar)
{
	int event;
	
	moon_pushlightuserdata(L, &hook_index);
	moon_rawget(L, LUA_REGISTRYINDEX);

	event = ar->event;
	moon_pushstring(L, hooknames[event]);
  
	moon_getinfo(L, "nS", ar);
	if (*(ar->what) == 'C') {
		moon_pushfstring(L, "[?%s]", ar->name);
	} else {
		moon_pushfstring(L, "%s:%d", ar->short_src, ar->linedefined > 0 ? ar->linedefined : 0);
	}

	lua_call(L, 2, 0);
}

static void call_ret_hook(lua_State *L) {
	lua_Debug ar;
	
	if (moon_gethook(L)) {
		moon_getstack(L, 0, &ar);
		moon_getinfo(L, "n", &ar);
		
		moon_pushlightuserdata(L, &hook_index);
		moon_rawget(L, LUA_REGISTRYINDEX);
		
		if (moon_type(L, -1) != LUA_TFUNCTION){
			lua_pop(L, 1);
			return;
        }
		
		lua_pushliteral(L, "return");
		moon_pushfstring(L, "[?%s]", ar.name);
		lua_pushliteral(L, "[C#]");
		
		moon_sethook(L, 0, 0, 0);
		lua_call(L, 3, 0);
		moon_sethook(L, hook, LUA_MASKCALL | LUA_MASKRET, 0);
	}
}

static int profiler_set_hook(lua_State *L) {
	if (lua_isnoneornil(L, 1)) {
		moon_pushlightuserdata(L, &hook_index);
		moon_pushnil(L);
		moon_rawset(L, LUA_REGISTRYINDEX);
			
		moon_sethook(L, 0, 0, 0);
	} else {
		moonL_checktype(L, 1, LUA_TFUNCTION);
		moon_pushlightuserdata(L, &hook_index);
		moon_pushvalue(L, 1);
		moon_rawset(L, LUA_REGISTRYINDEX);
		moon_sethook(L, hook, LUA_MASKCALL | LUA_MASKRET, 0);
	}
	return 0;
}

static int csharp_function_wrap(lua_State *L) {
	lua_CFunction fn = (lua_CFunction)moon_tocfunction(L, lua_upvalueindex(1));
    int ret = fn(L);    
    
    if (moon_toboolean(L, lua_upvalueindex(2)))
    {
        moon_pushboolean(L, 0);
        moon_replace(L, lua_upvalueindex(2));
        return moon_error(L);
    }
    
	if (moon_gethook(L)) {
		call_ret_hook(L);
	}
	
    return ret;
}

LUA_API void xmoon_push_csharp_function(lua_State* L, lua_CFunction fn, int n)
{ 
    lua_pushcfunction(L, fn);
	if (n > 0) {
		moon_insert(L, -1 - n);
	}
	moon_pushboolean(L, 0);
	if (n > 0) {
		moon_insert(L, -1 - n);
	}
    moon_pushcclosure(L, csharp_function_wrap, 2 + (n > 0 ? n : 0));
}

typedef int (*lua_CSWrapperCaller) (lua_State *L, int wrapperid, int top);

static lua_CSWrapperCaller g_csharp_wrapper_caller = NULL;

LUA_API void xmoon_set_csharp_wrapper_caller(lua_CSWrapperCaller wrapper_caller)
{
	g_csharp_wrapper_caller = wrapper_caller;
}

static int csharp_function_wrapper_wrapper(lua_State *L) {
    int ret = 0;
	
	if (g_csharp_wrapper_caller == NULL) {
		return moonL_error(L, "g_csharp_wrapper_caller not set");
	}
	
	ret = g_csharp_wrapper_caller(L, xmoon_tointeger(L, lua_upvalueindex(1)), moon_gettop(L));    
    
    if (moon_toboolean(L, lua_upvalueindex(2)))
    {
        moon_pushboolean(L, 0);
        moon_replace(L, lua_upvalueindex(2));
        return moon_error(L);
    }
    
	if (moon_gethook(L)) {
		call_ret_hook(L);
	}
	
    return ret;
}

LUA_API void xmoon_push_csharp_wrapper(lua_State* L, int wrapperid)
{ 
	moon_pushinteger(L, wrapperid);
	moon_pushboolean(L, 0);
    moon_pushcclosure(L, csharp_function_wrapper_wrapper, 2);
}

LUALIB_API int xmoon_upvalueindex(int n) {
	return lua_upvalueindex(2 + n);
}

LUALIB_API int xmoon_csharp_str_error(lua_State* L, const char* msg)
{
    moon_pushboolean(L, 1);
    moon_replace(L, lua_upvalueindex(2));
    moon_pushstring(L, msg);
    return 1;
}

LUALIB_API int xmoon_csharp_error(lua_State* L)
{
    moon_pushboolean(L, 1);
    moon_replace(L, lua_upvalueindex(2));
    return 1;
}

typedef struct {
	int fake_id;
    unsigned int len;
	char data[1];
} CSharpStruct;

LUA_API void *xmoon_pushstruct(lua_State *L, unsigned int size, int meta_ref) {
	CSharpStruct *css = (CSharpStruct *)lua_newuserdata(L, size + sizeof(int) + sizeof(unsigned int));
	css->fake_id = -1;
	css->len = size;
    moon_rawgeti(L, LUA_REGISTRYINDEX, meta_ref);
	moon_setmetatable(L, -2);
	return css;
}

LUA_API void xmoon_pushcstable(lua_State *L, unsigned int size, int meta_ref) {
	moon_createtable(L, 0, size);
    moon_rawgeti(L, LUA_REGISTRYINDEX, meta_ref);
	moon_setmetatable(L, -2);
}

LUA_API void *xmoon_newstruct(lua_State *L, int size, int meta_ref) {
	CSharpStruct *css = (CSharpStruct *)lua_newuserdata(L, size + sizeof(int) + sizeof(unsigned int));
	css->fake_id = -1;
	css->len = size;
    moon_rawgeti(L, LUA_REGISTRYINDEX, meta_ref);
	moon_setmetatable(L, -2);
	return css->data;
}

LUA_API void *xmoon_tostruct(lua_State *L, int idx, int meta_ref) {
	CSharpStruct *css = (CSharpStruct *)moon_touserdata(L, idx);
	if (NULL != css) {
		if (moon_getmetatable (L, idx)) {
			moon_rawgeti(L, -1, 1);
			if (moon_type(L, -1) == LUA_TNUMBER && (int)lua_tointeger(L, -1) == meta_ref) {
				lua_pop(L, 2);
				return css->data; 
			}
			lua_pop(L, 2);
		}
	}
	return NULL;
}

LUA_API int xmoon_gettypeid(lua_State *L, int idx) {
	int type_id = -1;
	if (moon_type(L, idx) == LUA_TUSERDATA) {
		if (moon_getmetatable (L, idx)) {
			moon_rawgeti(L, -1, 1);
			if (moon_type(L, -1) == LUA_TNUMBER) {
				type_id = (int)lua_tointeger(L, -1);
			}
			lua_pop(L, 2);
		}
	}
	return type_id;
}

#define PACK_UNPACK_OF(type) \
LUALIB_API int xmoon_pack_##type(void *p, int offset, type field) {\
	CSharpStruct *css = (CSharpStruct *)p;\
	if (css->fake_id != -1 || css->len < offset + sizeof(field)) {\
		return 0;\
	} else {\
		memcpy((&(css->data[0]) + offset), &field, sizeof(field));\
		return 1;\
	}\
}\
\
LUALIB_API int xmoon_unpack_##type(void *p, int offset, type *pfield) { \
	CSharpStruct *css = (CSharpStruct *)p;\
	if (css->fake_id != -1 || css->len < offset + sizeof(*pfield)) {\
		return 0;\
	} else {\
		memcpy(pfield, (&(css->data[0]) + offset), sizeof(*pfield));\
		return 1;\
	}\
}\

PACK_UNPACK_OF(int8_t);
PACK_UNPACK_OF(int16_t);
PACK_UNPACK_OF(int32_t);
PACK_UNPACK_OF(int64_t);
PACK_UNPACK_OF(float);
PACK_UNPACK_OF(double);

LUALIB_API int xmoon_pack_float2(void *p, int offset, float f1, float f2) {
	CSharpStruct *css = (CSharpStruct *)p;
	if (css->fake_id != -1 || css->len < offset + sizeof(float) * 2) {
		return 0;
	} else {
		float *pos = (float *)(&(css->data[0]) + offset);
		pos[0] = f1;
		pos[1] = f2;
		return 1;
	}
}

LUALIB_API int xmoon_unpack_float2(void *p, int offset, float *f1, float *f2) {
	CSharpStruct *css = (CSharpStruct *)p;
	if (css->fake_id != -1 || css->len < offset + sizeof(float) * 2) {
		return 0;
	} else {
		float *pos = (float *)(&(css->data[0]) + offset);
		*f1 = pos[0];
		*f2 = pos[1];
		return 1;
	}
}

LUALIB_API int xmoon_pack_float3(void *p, int offset, float f1, float f2, float f3) {
	CSharpStruct *css = (CSharpStruct *)p;
	if (css->fake_id != -1 || css->len < offset + sizeof(float) * 3) {
		return 0;
	} else {
		float *pos = (float *)(&(css->data[0]) + offset);
		pos[0] = f1;
		pos[1] = f2;
		pos[2] = f3;
		return 1;
	}
}

LUALIB_API int xmoon_unpack_float3(void *p, int offset, float *f1, float *f2, float *f3) {
	CSharpStruct *css = (CSharpStruct *)p;
	if (css->fake_id != -1 || css->len < offset + sizeof(float) * 3) {
		return 0;
	} else {
		float *pos = (float *)(&(css->data[0]) + offset);
		*f1 = pos[0];
		*f2 = pos[1];
		*f3 = pos[2];
		return 1;
	}
}

LUALIB_API int xmoon_pack_float4(void *p, int offset, float f1, float f2, float f3, float f4) {
	CSharpStruct *css = (CSharpStruct *)p;
	if (css->fake_id != -1 || css->len < offset + sizeof(float) * 4) {
		return 0;
	} else {
		float *pos = (float *)(&(css->data[0]) + offset);
		pos[0] = f1;
		pos[1] = f2;
		pos[2] = f3;
		pos[3] = f4;
		return 1;
	}
}

LUALIB_API int xmoon_unpack_float4(void *p, int offset, float *f1, float *f2, float *f3, float *f4) {
	CSharpStruct *css = (CSharpStruct *)p;
	if (css->fake_id != -1 || css->len < offset + sizeof(float) * 4) {
		return 0;
	} else {
		float *pos = (float *)(&(css->data[0]) + offset);
		*f1 = pos[0];
		*f2 = pos[1];
		*f3 = pos[2];
		*f4 = pos[3];
		return 1;
	}
}

LUALIB_API int xmoon_pack_float5(void *p, int offset, float f1, float f2, float f3, float f4, float f5) {
	CSharpStruct *css = (CSharpStruct *)p;
	if (css->fake_id != -1 || css->len < offset + sizeof(float) * 5) {
		return 0;
	} else {
		float *pos = (float *)(&(css->data[0]) + offset);
		pos[0] = f1;
		pos[1] = f2;
		pos[2] = f3;
		pos[3] = f4;
		pos[4] = f5;
		return 1;
	}
}

LUALIB_API int xmoon_unpack_float5(void *p, int offset, float *f1, float *f2, float *f3, float *f4, float *f5) {
	CSharpStruct *css = (CSharpStruct *)p;
	if (css->fake_id != -1 || css->len < offset + sizeof(float) * 5) {
		return 0;
	} else {
		float *pos = (float *)(&(css->data[0]) + offset);
		*f1 = pos[0];
		*f2 = pos[1];
		*f3 = pos[2];
		*f4 = pos[3];
		*f5 = pos[4];
		return 1;
	}
}

LUALIB_API int xmoon_pack_float6(void *p, int offset, float f1, float f2, float f3, float f4, float f5, float f6) {
	CSharpStruct *css = (CSharpStruct *)p;
	if (css->fake_id != -1 || css->len < offset + sizeof(float) * 6) {
		return 0;
	} else {
		float *pos = (float *)(&(css->data[0]) + offset);
		pos[0] = f1;
		pos[1] = f2;
		pos[2] = f3;
		pos[3] = f4;
		pos[4] = f5;
		pos[5] = f6;
		return 1;
	}
}

LUALIB_API int xmoon_unpack_float6(void *p, int offset, float *f1, float *f2, float *f3, float *f4, float *f5, float *f6) {
	CSharpStruct *css = (CSharpStruct *)p;
	if (css->fake_id != -1 || css->len < offset + sizeof(float) * 6) {
		return 0;
	} else {
		float *pos = (float *)(&(css->data[0]) + offset);
		*f1 = pos[0];
		*f2 = pos[1];
		*f3 = pos[2];
		*f4 = pos[3];
		*f5 = pos[4];
		*f6 = pos[5];
		return 1;
	}
}

LUALIB_API int xmoon_pack_decimal(void *p, int offset, const int * decimal) {
	CSharpStruct *css = (CSharpStruct *)p;
	if (css->fake_id != -1 || css->len < sizeof(int) * 4) {
		return 0;
	} else {
		int *pos = (int *)(&(css->data[0]) + offset);
		pos[0] = decimal[0];
		pos[1] = decimal[1];
		pos[2] = decimal[2];
		pos[3] = decimal[3];
		return 1;
	}
}

typedef struct tagDEC {
    uint16_t    wReserved;
    uint8_t     scale;
    uint8_t     sign;
    int         Hi32;
    uint64_t    Lo64;
} DECIMAL;

LUALIB_API int xmoon_unpack_decimal(void *p, int offset, uint8_t *scale, uint8_t *sign, int *hi32, uint64_t *lo64) {
	CSharpStruct *css = (CSharpStruct *)p;
	if (css->fake_id != -1 || css->len < sizeof(int) * 4) {
		return 0;
	} else {
		DECIMAL *dec = (DECIMAL *)(&(css->data[0]) + offset);
		*scale = dec->scale;
		*sign = dec->sign;
		*hi32 = dec->Hi32;
		*lo64 = dec->Lo64;
		return 1;
	}
}

LUA_API int xmoon_is_eq_str(lua_State *L, int idx, const char* str, int str_len) {
	size_t lmsg;
    const char *msg;
	if (moon_type(L, idx) == LUA_TSTRING) {
        msg = moon_tolstring(L, idx, &lmsg);
		return (lmsg == str_len) && (memcmp(msg, str, lmsg) == 0);
	} else {
		return 0;
	}
}

#define T_INT8   0
#define T_UINT8  1
#define T_INT16  2
#define T_UINT16 3
#define T_INT32  4
#define T_UINT32 5
#define T_INT64  6
#define T_UINT64 7
#define T_FLOAT  8
#define T_DOUBLE 9

#define DIRECT_ACCESS(type, push_func, to_func) \
int xmoon_struct_get_##type(lua_State *L) {\
	CSharpStruct *css = (CSharpStruct *)moon_touserdata(L, 1);\
	int offset = xmoon_tointeger(L, lua_upvalueindex(1));\
	type val;\
	if (css == NULL || css->fake_id != -1 || css->len < offset + sizeof(type)) {\
		return moonL_error(L, "invalid c# struct!");\
	} else {\
		memcpy(&val, (&(css->data[0]) + offset), sizeof(type));\
		push_func(L, val);\
		return 1;\
	}\
}\
\
int xmoon_struct_set_##type(lua_State *L) { \
	CSharpStruct *css = (CSharpStruct *)moon_touserdata(L, 1);\
	int offset = xmoon_tointeger(L, lua_upvalueindex(1));\
	type val;\
	if (css == NULL || css->fake_id != -1 || css->len < offset + sizeof(type)) {\
		return moonL_error(L, "invalid c# struct!");\
	} else {\
	    val = (type)to_func(L, 2);\
		memcpy((&(css->data[0]) + offset), &val, sizeof(type));\
		return 0;\
	}\
}\

DIRECT_ACCESS(int8_t, xmoon_pushinteger, xmoon_tointeger);
DIRECT_ACCESS(uint8_t, xmoon_pushinteger, xmoon_tointeger);
DIRECT_ACCESS(int16_t, xmoon_pushinteger, xmoon_tointeger);
DIRECT_ACCESS(uint16_t, xmoon_pushinteger, xmoon_tointeger);
DIRECT_ACCESS(int32_t, xmoon_pushinteger, xmoon_tointeger);
DIRECT_ACCESS(uint32_t, xmoon_pushuint, xmoon_touint);
DIRECT_ACCESS(int64_t, moon_pushint64, moon_toint64);
DIRECT_ACCESS(uint64_t, moon_pushuint64, moon_touint64);
DIRECT_ACCESS(float, moon_pushnumber, moon_tonumber);
DIRECT_ACCESS(double, moon_pushnumber, moon_tonumber);

static const lua_CFunction direct_getters[10] = {
	xmoon_struct_get_int8_t,
	xmoon_struct_get_uint8_t,
	xmoon_struct_get_int16_t,
	xmoon_struct_get_uint16_t,
	xmoon_struct_get_int32_t,
	xmoon_struct_get_uint32_t,
	xmoon_struct_get_int64_t,
	xmoon_struct_get_uint64_t,
	xmoon_struct_get_float,
	xmoon_struct_get_double
};

static const lua_CFunction direct_setters[10] = {
	xmoon_struct_set_int8_t,
	xmoon_struct_set_uint8_t,
	xmoon_struct_set_int16_t,
	xmoon_struct_set_uint16_t,
	xmoon_struct_set_int32_t,
	xmoon_struct_set_uint32_t,
	xmoon_struct_set_int64_t,
	xmoon_struct_set_uint64_t,
	xmoon_struct_set_float,
	xmoon_struct_set_double
};

int nop(lua_State *L) {
	return 0;
}

LUA_API int gen_css_access(lua_State *L) {
	int offset = xmoon_tointeger(L, 1);
	int type = xmoon_tointeger(L, 2);
	if (offset < 0) {
		return moonL_error(L, "offset must larger than 0");
	}
	if (type < T_INT8 || type > T_DOUBLE) {
		return moonL_error(L, "unknow tag[%d]", type);
	}
	moon_pushvalue(L, 1);
	moon_pushcclosure(L, direct_getters[type], 1);
	moon_pushvalue(L, 1);
	moon_pushcclosure(L, direct_setters[type], 1);
	moon_pushcclosure(L, nop, 0);
	return 3;
}

static int is_cs_data(lua_State *L, int idx) {
	if (LUA_TUSERDATA == moon_type(L, idx) && moon_getmetatable(L, idx)) {
		moon_pushlightuserdata(L, &tag);
		moon_rawget(L,-2);
		if (!lua_isnil (L,-1)) {
			lua_pop (L, 2);
			return 1;
		}
		lua_pop (L, 2);
	}
	return 0;
}

LUA_API int css_clone(lua_State *L) {
	CSharpStruct *from = (CSharpStruct *)moon_touserdata(L, 1);
	CSharpStruct *to = NULL;
	if (!is_cs_data(L, 1) || from->fake_id != -1) {
		return moonL_error(L, "invalid c# struct!");
	}
	
	to = (CSharpStruct *)lua_newuserdata(L, from->len + sizeof(int) + sizeof(unsigned int));
	to->fake_id = -1;
	to->len = from->len;
	memcpy(&(to->data[0]), &(from->data[0]), from->len);
    moon_getmetatable(L, 1);
	moon_setmetatable(L, -2);
	return 1;
}

LUA_API void* xmoon_gl(lua_State *L) {
	return G(L);
}

static const luaL_Reg xlualib[] = {
	{"sethook", profiler_set_hook},
	{"genaccessor", gen_css_access},
	{"structclone", css_clone},
	{NULL, NULL}
};

LUA_API void moonopen_xlua(lua_State *L) {
	moonL_openlibs(L);
	
#if LUA_VERSION_NUM >= 503
	luaL_newlib(L, xlualib);
	moon_setglobal(L, "xlua");
#else
	luaL_register(L, "xlua", xlualib);
    lua_pop(L, 1);
#endif
}

