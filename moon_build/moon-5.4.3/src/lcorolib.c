/*
** $Id: lcorolib.c $
** Coroutine Library
** See Copyright Notice in lua.h
*/

#define lcorolib_c
#define LUA_LIB

#include "lprefix.h"


#include <stdlib.h>

#include "lua.h"

#include "lauxlib.h"
#include "lualib.h"


static lua_State *getco (lua_State *L) {
  lua_State *co = moon_tothread(L, 1);
  luaL_argexpected(L, co, 1, "thread");
  return co;
}


/*
** Resumes a coroutine. Returns the number of results for non-error
** cases or -1 for errors.
*/
static int auxresume (lua_State *L, lua_State *co, int narg) {
  int status, nres;
  if (l_unlikely(!moon_checkstack(co, narg))) {
    lua_pushliteral(L, "too many arguments to resume");
    return -1;  /* error flag */
  }
  moon_xmove(L, co, narg);
  status = moon_resume(co, L, narg, &nres);
  if (l_likely(status == LUA_OK || status == LUA_YIELD)) {
    if (l_unlikely(!moon_checkstack(L, nres + 1))) {
      lua_pop(co, nres);  /* remove results anyway */
      lua_pushliteral(L, "too many results to resume");
      return -1;  /* error flag */
    }
    moon_xmove(co, L, nres);  /* move yielded values */
    return nres;
  }
  else {
    moon_xmove(co, L, 1);  /* move error message */
    return -1;  /* error flag */
  }
}


static int moonB_coresume (lua_State *L) {
  lua_State *co = getco(L);
  int r;
  r = auxresume(L, co, moon_gettop(L) - 1);
  if (l_unlikely(r < 0)) {
    moon_pushboolean(L, 0);
    moon_insert(L, -2);
    return 2;  /* return false + error message */
  }
  else {
    moon_pushboolean(L, 1);
    moon_insert(L, -(r + 1));
    return r + 1;  /* return true + 'resume' returns */
  }
}


static int moonB_auxwrap (lua_State *L) {
  lua_State *co = moon_tothread(L, lua_upvalueindex(1));
  int r = auxresume(L, co, moon_gettop(L));
  if (l_unlikely(r < 0)) {  /* error? */
    int stat = moon_status(co);
    if (stat != LUA_OK && stat != LUA_YIELD) {  /* error in the coroutine? */
      stat = moon_resetthread(co);  /* close its tbc variables */
      lua_assert(stat != LUA_OK);
      moon_xmove(co, L, 1);  /* move error message to the caller */
    }
    if (stat != LUA_ERRMEM &&  /* not a memory error and ... */
        moon_type(L, -1) == LUA_TSTRING) {  /* ... error object is a string? */
      moonL_where(L, 1);  /* add extra info, if available */
      moon_insert(L, -2);
      moon_concat(L, 2);
    }
    return moon_error(L);  /* propagate error */
  }
  return r;
}


static int moonB_cocreate (lua_State *L) {
  lua_State *NL;
  moonL_checktype(L, 1, LUA_TFUNCTION);
  NL = moon_newthread(L);
  moon_pushvalue(L, 1);  /* move function to top */
  moon_xmove(L, NL, 1);  /* move function from L to NL */
  return 1;
}


static int moonB_cowrap (lua_State *L) {
  moonB_cocreate(L);
  moon_pushcclosure(L, moonB_auxwrap, 1);
  return 1;
}


static int moonB_yield (lua_State *L) {
  return lua_yield(L, moon_gettop(L));
}


#define COS_RUN		0
#define COS_DEAD	1
#define COS_YIELD	2
#define COS_NORM	3


static const char *const statname[] =
  {"running", "dead", "suspended", "normal"};


static int auxstatus (lua_State *L, lua_State *co) {
  if (L == co) return COS_RUN;
  else {
    switch (moon_status(co)) {
      case LUA_YIELD:
        return COS_YIELD;
      case LUA_OK: {
        lua_Debug ar;
        if (moon_getstack(co, 0, &ar))  /* does it have frames? */
          return COS_NORM;  /* it is running */
        else if (moon_gettop(co) == 0)
            return COS_DEAD;
        else
          return COS_YIELD;  /* initial state */
      }
      default:  /* some error occurred */
        return COS_DEAD;
    }
  }
}


static int moonB_costatus (lua_State *L) {
  lua_State *co = getco(L);
  moon_pushstring(L, statname[auxstatus(L, co)]);
  return 1;
}


static int moonB_yieldable (lua_State *L) {
  lua_State *co = lua_isnone(L, 1) ? L : getco(L);
  moon_pushboolean(L, moon_isyieldable(co));
  return 1;
}


static int moonB_corunning (lua_State *L) {
  int ismain = moon_pushthread(L);
  moon_pushboolean(L, ismain);
  return 2;
}


static int moonB_close (lua_State *L) {
  lua_State *co = getco(L);
  int status = auxstatus(L, co);
  switch (status) {
    case COS_DEAD: case COS_YIELD: {
      status = moon_resetthread(co);
      if (status == LUA_OK) {
        moon_pushboolean(L, 1);
        return 1;
      }
      else {
        moon_pushboolean(L, 0);
        moon_xmove(co, L, 1);  /* move error message */
        return 2;
      }
    }
    default:  /* normal or running coroutine */
      return moonL_error(L, "cannot close a %s coroutine", statname[status]);
  }
}


static const luaL_Reg co_funcs[] = {
  {"create", moonB_cocreate},
  {"resume", moonB_coresume},
  {"running", moonB_corunning},
  {"status", moonB_costatus},
  {"wrap", moonB_cowrap},
  {"yield", moonB_yield},
  {"isyieldable", moonB_yieldable},
  {"close", moonB_close},
  {NULL, NULL}
};



LUAMOD_API int moonopen_coroutine (lua_State *L) {
  luaL_newlib(L, co_funcs);
  return 1;
}

