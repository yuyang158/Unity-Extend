﻿/*
** $Id: linit.c $
** Initialization of libraries for lua.c and other clients
** See Copyright Notice in lua.h
*/


#define linit_c
#define LUA_LIB

/*
** If you embed Lua in your program and need to open the standard
** libraries, call luaL_openlibs in your program. If you need a
** different set of libraries, copy this file to your project and edit
** it to suit your needs.
**
** You can also *preload* libraries, so that a later 'require' can
** open the library, which is already linked to the application.
** For that, do the following code:
**
**  luaL_getsubtable(L, LUA_REGISTRYINDEX, LUA_PRELOAD_TABLE);
**  lua_pushcfunction(L, luaopen_modname);
**  lua_setfield(L, -2, modname);
**  lua_pop(L, 1);  // remove PRELOAD table
*/

#include "lprefix.h"


#include <stddef.h>

#include "lua.h"

#include "lualib.h"
#include "lauxlib.h"


/*
** these libs are loaded by lua.c and are readily available to any Lua
** program
*/
static const luaL_Reg loadedlibs[] = {
  {LUA_GNAME, moonopen_base},
  {LUA_LOADLIBNAME, moonopen_package},
  {LUA_COLIBNAME, moonopen_coroutine},
  {LUA_TABLIBNAME, moonopen_table},
  {LUA_IOLIBNAME, moonopen_io},
  {LUA_OSLIBNAME, moonopen_os},
  {LUA_STRLIBNAME, moonopen_string},
  {LUA_MATHLIBNAME, moonopen_math},
  {LUA_UTF8LIBNAME, moonopen_utf8},
  {LUA_DBLIBNAME, moonopen_debug},
  {NULL, NULL}
};


LUALIB_API void moonL_openlibs (lua_State *L) {
  const luaL_Reg *lib;
  /* "require" functions from 'loadedlibs' and set results to global table */
  for (lib = loadedlibs; lib->func; lib++) {
    moonL_requiref(L, lib->name, lib->func, 1);
    lua_pop(L, 1);  /* remove lib */
  }
}

