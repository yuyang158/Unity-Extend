/*
** $Id: lualib.h $
** Lua standard libraries
** See Copyright Notice in lua.h
*/


#ifndef lualib_h
#define lualib_h

#include "lua.h"


/* version suffix for environment variable names */
#define LUA_VERSUFFIX          "_" LUA_VERSION_MAJOR "_" LUA_VERSION_MINOR


LUAMOD_API int (moonopen_base) (lua_State *L);

#define LUA_COLIBNAME	"coroutine"
LUAMOD_API int (moonopen_coroutine) (lua_State *L);

#define LUA_TABLIBNAME	"table"
LUAMOD_API int (moonopen_table) (lua_State *L);

#define LUA_IOLIBNAME	"io"
LUAMOD_API int (moonopen_io) (lua_State *L);

#define LUA_OSLIBNAME	"os"
LUAMOD_API int (moonopen_os) (lua_State *L);

#define LUA_STRLIBNAME	"string"
LUAMOD_API int (moonopen_string) (lua_State *L);

#define LUA_UTF8LIBNAME	"utf8"
LUAMOD_API int (moonopen_utf8) (lua_State *L);

#define LUA_MATHLIBNAME	"math"
LUAMOD_API int (moonopen_math) (lua_State *L);

#define LUA_DBLIBNAME	"debug"
LUAMOD_API int (moonopen_debug) (lua_State *L);

#define LUA_LOADLIBNAME	"package"
LUAMOD_API int (moonopen_package) (lua_State *L);


/* open all previous libraries */
LUALIB_API void (moonL_openlibs) (lua_State *L);


#endif
