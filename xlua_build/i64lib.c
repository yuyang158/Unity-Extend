/*
 *Tencent is pleased to support the open source community by making xLua available.
 *Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 *Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 *http://opensource.org/licenses/MIT
 *Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

#define LUA_LIB

#include "i64lib.h"
#include <string.h>
#include <math.h>
#include <stdlib.h>

#if ( defined (_WIN32) ||  defined (_WIN64) ) && !defined (__MINGW32__) && !defined (__MINGW64__)

#if !defined(PRId64)
# if __WORDSIZE == 64  
#  define PRId64    "ld"   
# else  
#  define PRId64    "lld"  
# endif 
#endif

#if !defined(PRIu64)
# if __WORDSIZE == 64  
#  define PRIu64    "lu"   
# else  
#  define PRIu64    "llu"  
# endif
#endif

#else
#include <inttypes.h>
#endif

#if LUA_VERSION_NUM >= 503
LUALIB_API void moon_pushint64(lua_State* L, int64_t n) {
	moon_pushinteger(L, n);
}
LUALIB_API void moon_pushuint64(lua_State* L, uint64_t n) {
	moon_pushinteger(L, n);
}

LUALIB_API int moon_isint64(lua_State* L, int pos) {
	return moon_isinteger(L, pos);
}

LUALIB_API int moon_isuint64(lua_State* L, int pos) {
	return moon_isinteger(L, pos);
}

LUALIB_API int64_t moon_toint64(lua_State* L, int pos) {
	return lua_tointeger(L, pos);
}

LUALIB_API uint64_t moon_touint64(lua_State* L, int pos) {
	return lua_tointeger(L, pos);
}
#endif

static int uint64_tostring(lua_State* L) {
	char temp[72];
	uint64_t n = moon_touint64(L, 1);
#if ( defined (_WIN32) ||  defined (_WIN64) ) && !defined (__MINGW32__) && !defined (__MINGW64__)
	sprintf_s(temp, sizeof(temp), "%"PRIu64, n);
#else
	snprintf(temp, sizeof(temp), "%"PRIu64, n);
#endif
	
	moon_pushstring(L, temp);
	
	return 1;
}

static int uint64_compare(lua_State* L) {
	uint64_t lhs = moon_touint64(L, 1);
	uint64_t rhs = moon_touint64(L, 2);
	moon_pushinteger(L, lhs == rhs ? 0 : (lhs < rhs ? -1 : 1));
	return 1;
}

static int uint64_divide(lua_State* L) {
	uint64_t lhs = moon_touint64(L, 1);
	uint64_t rhs = moon_touint64(L, 2);
	if (rhs == 0) {
        return moonL_error(L, "div by zero");
    }
    moon_pushuint64(L, lhs / rhs);
	return 1;
}

static int uint64_remainder(lua_State* L) {
	uint64_t lhs = moon_touint64(L, 1);
	uint64_t rhs = moon_touint64(L, 2);
	if (rhs == 0) {
        return moonL_error(L, "div by zero");
    }
    moon_pushuint64(L, lhs % rhs);
	return 1;
}

LUALIB_API int uint64_parse(lua_State* L)
{
    const char* str = lua_tostring(L, 1);
    moon_pushuint64(L, strtoull(str, NULL, 0));
    return 1;
}

LUALIB_API int moonopen_i64lib(lua_State* L)
{
#if LUA_VERSION_NUM == 501
    lua_newtable(L);
	
    lua_pushcfunction(L, int64_add);
    lua_setfield(L, -2, "__add");

    lua_pushcfunction(L, int64_sub);
    lua_setfield(L, -2, "__sub");

    lua_pushcfunction(L, int64_mul);
    lua_setfield(L, -2, "__mul");

    lua_pushcfunction(L, int64_div);
    lua_setfield(L, -2, "__div");

    lua_pushcfunction(L, int64_mod);
    lua_setfield(L, -2, "__mod");

    lua_pushcfunction(L, int64_unm);
    lua_setfield(L, -2, "__unm");

    lua_pushcfunction(L, int64_pow);
    lua_setfield(L, -2, "__pow");    

    lua_pushcfunction(L, int64_tostring);
    lua_setfield(L, -2, "__tostring");        

    lua_pushcfunction(L, int64_eq);
    lua_setfield(L, -2, "__eq");  

    lua_pushcfunction(L, int64_lt);
    lua_setfield(L, -2, "__lt"); 

    lua_pushcfunction(L, int64_le);
    lua_setfield(L, -2, "__le");
	
	lua_rawseti(L, LUA_REGISTRYINDEX, INT64_META_REF);
#endif
    lua_newtable(L);
	
	lua_pushcfunction(L, uint64_tostring);
	moon_setfield(L, -2, "tostring");
	
	lua_pushcfunction(L, uint64_compare);
	moon_setfield(L, -2, "compare");
	
	lua_pushcfunction(L, uint64_divide);
	moon_setfield(L, -2, "divide");
	
	lua_pushcfunction(L, uint64_remainder);
	moon_setfield(L, -2, "remainder");
	
	lua_pushcfunction(L, uint64_parse);
	moon_setfield(L, -2, "parse");
	
	moon_setglobal(L, "uint64");
	return 0;
}

