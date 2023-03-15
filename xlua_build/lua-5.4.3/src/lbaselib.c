/*
** $Id: lbaselib.c $
** Basic library
** See Copyright Notice in lua.h
*/

#define lbaselib_c
#define LUA_LIB

#include "lprefix.h"


#include <ctype.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include "lua.h"

#include "lauxlib.h"
#include "lualib.h"


static int moonB_print (lua_State *L) {
  int n = moon_gettop(L);  /* number of arguments */
  int i;
  for (i = 1; i <= n; i++) {  /* for each argument */
    size_t l;
    const char *s = moonL_tolstring(L, i, &l);  /* convert it to string */
    if (i > 1)  /* not the first element? */
      lua_writestring("\t", 1);  /* add a tab before it */
    lua_writestring(s, l);  /* print it */
    lua_pop(L, 1);  /* pop result */
  }
  lua_writeline();
  return 0;
}


/*
** Creates a warning with all given arguments.
** Check first for errors; otherwise an error may interrupt
** the composition of a warning, leaving it unfinished.
*/
static int moonB_warn (lua_State *L) {
  int n = moon_gettop(L);  /* number of arguments */
  int i;
  luaL_checkstring(L, 1);  /* at least one argument */
  for (i = 2; i <= n; i++)
    luaL_checkstring(L, i);  /* make sure all arguments are strings */
  for (i = 1; i < n; i++)  /* compose warning */
    moon_warning(L, lua_tostring(L, i), 1);
  moon_warning(L, lua_tostring(L, n), 0);  /* close warning */
  return 0;
}


#define SPACECHARS	" \f\n\r\t\v"

static const char *b_str2int (const char *s, int base, lua_Integer *pn) {
  lua_Unsigned n = 0;
  int neg = 0;
  s += strspn(s, SPACECHARS);  /* skip initial spaces */
  if (*s == '-') { s++; neg = 1; }  /* handle sign */
  else if (*s == '+') s++;
  if (!isalnum((unsigned char)*s))  /* no digit? */
    return NULL;
  do {
    int digit = (isdigit((unsigned char)*s)) ? *s - '0'
                   : (toupper((unsigned char)*s) - 'A') + 10;
    if (digit >= base) return NULL;  /* invalid numeral */
    n = n * base + digit;
    s++;
  } while (isalnum((unsigned char)*s));
  s += strspn(s, SPACECHARS);  /* skip trailing spaces */
  *pn = (lua_Integer)((neg) ? (0u - n) : n);
  return s;
}


static int moonB_tonumber (lua_State *L) {
  if (lua_isnoneornil(L, 2)) {  /* standard conversion? */
    if (moon_type(L, 1) == LUA_TNUMBER) {  /* already a number? */
      moon_settop(L, 1);  /* yes; return it */
      return 1;
    }
    else {
      size_t l;
      const char *s = moon_tolstring(L, 1, &l);
      if (s != NULL && moon_stringtonumber(L, s) == l + 1)
        return 1;  /* successful conversion to number */
      /* else not a number */
      moonL_checkany(L, 1);  /* (but there must be some parameter) */
    }
  }
  else {
    size_t l;
    const char *s;
    lua_Integer n = 0;  /* to avoid warnings */
    lua_Integer base = moonL_checkinteger(L, 2);
    moonL_checktype(L, 1, LUA_TSTRING);  /* no numbers as strings */
    s = moon_tolstring(L, 1, &l);
    luaL_argcheck(L, 2 <= base && base <= 36, 2, "base out of range");
    if (b_str2int(s, (int)base, &n) == s + l) {
      moon_pushinteger(L, n);
      return 1;
    }  /* else not a number */
  }  /* else not a number */
  luaL_pushfail(L);  /* not a number */
  return 1;
}


static int moonB_error (lua_State *L) {
  int level = (int)moonL_optinteger(L, 2, 1);
  moon_settop(L, 1);
  if (moon_type(L, 1) == LUA_TSTRING && level > 0) {
    moonL_where(L, level);   /* add extra information */
    moon_pushvalue(L, 1);
    moon_concat(L, 2);
  }
  return moon_error(L);
}


static int moonB_getmetatable (lua_State *L) {
  moonL_checkany(L, 1);
  if (!moon_getmetatable(L, 1)) {
    moon_pushnil(L);
    return 1;  /* no metatable */
  }
  moonL_getmetafield(L, 1, "__metatable");
  return 1;  /* returns either __metatable field (if present) or metatable */
}


static int moonB_setmetatable (lua_State *L) {
  int t = moon_type(L, 2);
  moonL_checktype(L, 1, LUA_TTABLE);
  luaL_argexpected(L, t == LUA_TNIL || t == LUA_TTABLE, 2, "nil or table");
  if (l_unlikely(moonL_getmetafield(L, 1, "__metatable") != LUA_TNIL))
    return moonL_error(L, "cannot change a protected metatable");
  moon_settop(L, 2);
  moon_setmetatable(L, 1);
  return 1;
}


static int moonB_rawequal (lua_State *L) {
  moonL_checkany(L, 1);
  moonL_checkany(L, 2);
  moon_pushboolean(L, moon_rawequal(L, 1, 2));
  return 1;
}


static int moonB_rawlen (lua_State *L) {
  int t = moon_type(L, 1);
  luaL_argexpected(L, t == LUA_TTABLE || t == LUA_TSTRING, 1,
                      "table or string");
  moon_pushinteger(L, moon_rawlen(L, 1));
  return 1;
}


static int moonB_rawget (lua_State *L) {
  moonL_checktype(L, 1, LUA_TTABLE);
  moonL_checkany(L, 2);
  moon_settop(L, 2);
  moon_rawget(L, 1);
  return 1;
}

static int moonB_rawset (lua_State *L) {
  moonL_checktype(L, 1, LUA_TTABLE);
  moonL_checkany(L, 2);
  moonL_checkany(L, 3);
  moon_settop(L, 3);
  moon_rawset(L, 1);
  return 1;
}


static int pushmode (lua_State *L, int oldmode) {
  if (oldmode == -1)
    luaL_pushfail(L);  /* invalid call to 'lua_gc' */
  else
    moon_pushstring(L, (oldmode == LUA_GCINC) ? "incremental"
                                             : "generational");
  return 1;
}


/*
** check whether call to 'lua_gc' was valid (not inside a finalizer)
*/
#define checkvalres(res) { if (res == -1) break; }

static int moonB_collectgarbage (lua_State *L) {
  static const char *const opts[] = {"stop", "restart", "collect",
    "count", "step", "setpause", "setstepmul",
    "isrunning", "generational", "incremental", NULL};
  static const int optsnum[] = {LUA_GCSTOP, LUA_GCRESTART, LUA_GCCOLLECT,
    LUA_GCCOUNT, LUA_GCSTEP, LUA_GCSETPAUSE, LUA_GCSETSTEPMUL,
    LUA_GCISRUNNING, LUA_GCGEN, LUA_GCINC};
  int o = optsnum[moonL_checkoption(L, 1, "collect", opts)];
  switch (o) {
    case LUA_GCCOUNT: {
      int k = moon_gc(L, o);
      int b = moon_gc(L, LUA_GCCOUNTB);
      checkvalres(k);
      moon_pushnumber(L, (lua_Number)k + ((lua_Number)b/1024));
      return 1;
    }
    case LUA_GCSTEP: {
      int step = (int)moonL_optinteger(L, 2, 0);
      int res = moon_gc(L, o, step);
      checkvalres(res);
      moon_pushboolean(L, res);
      return 1;
    }
    case LUA_GCSETPAUSE:
    case LUA_GCSETSTEPMUL: {
      int p = (int)moonL_optinteger(L, 2, 0);
      int previous = moon_gc(L, o, p);
      checkvalres(previous);
      moon_pushinteger(L, previous);
      return 1;
    }
    case LUA_GCISRUNNING: {
      int res = moon_gc(L, o);
      checkvalres(res);
      moon_pushboolean(L, res);
      return 1;
    }
    case LUA_GCGEN: {
      int minormul = (int)moonL_optinteger(L, 2, 0);
      int majormul = (int)moonL_optinteger(L, 3, 0);
      return pushmode(L, moon_gc(L, o, minormul, majormul));
    }
    case LUA_GCINC: {
      int pause = (int)moonL_optinteger(L, 2, 0);
      int stepmul = (int)moonL_optinteger(L, 3, 0);
      int stepsize = (int)moonL_optinteger(L, 4, 0);
      return pushmode(L, moon_gc(L, o, pause, stepmul, stepsize));
    }
    default: {
      int res = moon_gc(L, o);
      checkvalres(res);
      moon_pushinteger(L, res);
      return 1;
    }
  }
  luaL_pushfail(L);  /* invalid call (inside a finalizer) */
  return 1;
}


static int moonB_type (lua_State *L) {
  int t = moon_type(L, 1);
  luaL_argcheck(L, t != LUA_TNONE, 1, "value expected");
  moon_pushstring(L, moon_typename(L, t));
  return 1;
}


static int moonB_next (lua_State *L) {
  moonL_checktype(L, 1, LUA_TTABLE);
  moon_settop(L, 2);  /* create a 2nd argument if there isn't one */
  if (moon_next(L, 1))
    return 2;
  else {
    moon_pushnil(L);
    return 1;
  }
}


static int pairscont (lua_State *L, int status, lua_KContext k) {
  (void)L; (void)status; (void)k;  /* unused */
  return 3;
}

static int moonB_pairs (lua_State *L) {
  moonL_checkany(L, 1);
  if (moonL_getmetafield(L, 1, "__pairs") == LUA_TNIL) {  /* no metamethod? */
    lua_pushcfunction(L, moonB_next);  /* will return generator, */
    moon_pushvalue(L, 1);  /* state, */
    moon_pushnil(L);  /* and initial value */
  }
  else {
    moon_pushvalue(L, 1);  /* argument 'self' to metamethod */
    moon_callk(L, 1, 3, 0, pairscont);  /* get 3 values from metamethod */
  }
  return 3;
}


/*
** Traversal function for 'ipairs'
*/
static int ipairsaux (lua_State *L) {
  lua_Integer i = moonL_checkinteger(L, 2);
  i = luaL_intop(+, i, 1);
  moon_pushinteger(L, i);
  return (moon_geti(L, 1, i) == LUA_TNIL) ? 1 : 2;
}


/*
** 'ipairs' function. Returns 'ipairsaux', given "table", 0.
** (The given "table" may not be a table.)
*/
static int moonB_ipairs (lua_State *L) {
  moonL_checkany(L, 1);
  lua_pushcfunction(L, ipairsaux);  /* iteration function */
  moon_pushvalue(L, 1);  /* state */
  moon_pushinteger(L, 0);  /* initial value */
  return 3;
}


static int load_aux (lua_State *L, int status, int envidx) {
  if (l_likely(status == LUA_OK)) {
    if (envidx != 0) {  /* 'env' parameter? */
      moon_pushvalue(L, envidx);  /* environment for loaded function */
      if (!moon_setupvalue(L, -2, 1))  /* set it as 1st upvalue */
        lua_pop(L, 1);  /* remove 'env' if not used by previous call */
    }
    return 1;
  }
  else {  /* error (message is on top of the stack) */
    luaL_pushfail(L);
    moon_insert(L, -2);  /* put before error message */
    return 2;  /* return fail plus error message */
  }
}


static int moonB_loadfile (lua_State *L) {
  const char *fname = luaL_optstring(L, 1, NULL);
  const char *mode = luaL_optstring(L, 2, NULL);
  int env = (!lua_isnone(L, 3) ? 3 : 0);  /* 'env' index or 0 if no 'env' */
  int status = moonL_loadfilex(L, fname, mode);
  return load_aux(L, status, env);
}


/*
** {======================================================
** Generic Read function
** =======================================================
*/


/*
** reserved slot, above all arguments, to hold a copy of the returned
** string to avoid it being collected while parsed. 'load' has four
** optional arguments (chunk, source name, mode, and environment).
*/
#define RESERVEDSLOT	5


/*
** Reader for generic 'load' function: 'lua_load' uses the
** stack for internal stuff, so the reader cannot change the
** stack top. Instead, it keeps its resulting string in a
** reserved slot inside the stack.
*/
static const char *generic_reader (lua_State *L, void *ud, size_t *size) {
  (void)(ud);  /* not used */
  moonL_checkstack(L, 2, "too many nested functions");
  moon_pushvalue(L, 1);  /* get function */
  lua_call(L, 0, 1);  /* call it */
  if (lua_isnil(L, -1)) {
    lua_pop(L, 1);  /* pop result */
    *size = 0;
    return NULL;
  }
  else if (l_unlikely(!moon_isstring(L, -1)))
    moonL_error(L, "reader function must return a string");
  moon_replace(L, RESERVEDSLOT);  /* save string in reserved slot */
  return moon_tolstring(L, RESERVEDSLOT, size);
}


static int moonB_load (lua_State *L) {
  int status;
  size_t l;
  const char *s = moon_tolstring(L, 1, &l);
  const char *mode = luaL_optstring(L, 3, "bt");
  int env = (!lua_isnone(L, 4) ? 4 : 0);  /* 'env' index or 0 if no 'env' */
  if (s != NULL) {  /* loading a string? */
    const char *chunkname = luaL_optstring(L, 2, s);
    status = moonL_loadbufferx(L, s, l, chunkname, mode);
  }
  else {  /* loading from a reader function */
    const char *chunkname = luaL_optstring(L, 2, "=(load)");
    moonL_checktype(L, 1, LUA_TFUNCTION);
    moon_settop(L, RESERVEDSLOT);  /* create reserved slot */
    status = moon_load(L, generic_reader, NULL, chunkname, mode);
  }
  return load_aux(L, status, env);
}

/* }====================================================== */


static int dofilecont (lua_State *L, int d1, lua_KContext d2) {
  (void)d1;  (void)d2;  /* only to match 'lua_Kfunction' prototype */
  return moon_gettop(L) - 1;
}


static int moonB_dofile (lua_State *L) {
  const char *fname = luaL_optstring(L, 1, NULL);
  moon_settop(L, 1);
  if (l_unlikely(luaL_loadfile(L, fname) != LUA_OK))
    return moon_error(L);
  moon_callk(L, 0, LUA_MULTRET, 0, dofilecont);
  return dofilecont(L, 0, 0);
}


static int moonB_assert (lua_State *L) {
  if (l_likely(moon_toboolean(L, 1)))  /* condition is true? */
    return moon_gettop(L);  /* return all arguments */
  else {  /* error */
    moonL_checkany(L, 1);  /* there must be a condition */
    moon_remove(L, 1);  /* remove it */
    lua_pushliteral(L, "assertion failed!");  /* default message */
    moon_settop(L, 1);  /* leave only message (default if no other one) */
    return moonB_error(L);  /* call 'error' */
  }
}


static int moonB_select (lua_State *L) {
  int n = moon_gettop(L);
  if (moon_type(L, 1) == LUA_TSTRING && *lua_tostring(L, 1) == '#') {
    moon_pushinteger(L, n-1);
    return 1;
  }
  else {
    lua_Integer i = moonL_checkinteger(L, 1);
    if (i < 0) i = n + i;
    else if (i > n) i = n;
    luaL_argcheck(L, 1 <= i, 1, "index out of range");
    return n - (int)i;
  }
}


/*
** Continuation function for 'pcall' and 'xpcall'. Both functions
** already pushed a 'true' before doing the call, so in case of success
** 'finishpcall' only has to return everything in the stack minus
** 'extra' values (where 'extra' is exactly the number of items to be
** ignored).
*/
static int finishpcall (lua_State *L, int status, lua_KContext extra) {
  if (l_unlikely(status != LUA_OK && status != LUA_YIELD)) {  /* error? */
    moon_pushboolean(L, 0);  /* first result (false) */
    moon_pushvalue(L, -2);  /* error message */
    return 2;  /* return false, msg */
  }
  else
    return moon_gettop(L) - (int)extra;  /* return all results */
}


static int moonB_pcall (lua_State *L) {
  int status;
  moonL_checkany(L, 1);
  moon_pushboolean(L, 1);  /* first result if no errors */
  moon_insert(L, 1);  /* put it in place */
  status = moon_pcallk(L, moon_gettop(L) - 2, LUA_MULTRET, 0, 0, finishpcall);
  return finishpcall(L, status, 0);
}


/*
** Do a protected call with error handling. After 'lua_rotate', the
** stack will have <f, err, true, f, [args...]>; so, the function passes
** 2 to 'finishpcall' to skip the 2 first values when returning results.
*/
static int moonB_xpcall (lua_State *L) {
  int status;
  int n = moon_gettop(L);
  moonL_checktype(L, 2, LUA_TFUNCTION);  /* check error function */
  moon_pushboolean(L, 1);  /* first result */
  moon_pushvalue(L, 1);  /* function */
  moon_rotate(L, 3, 2);  /* move them below function's arguments */
  status = moon_pcallk(L, n - 2, LUA_MULTRET, 2, 2, finishpcall);
  return finishpcall(L, status, 2);
}


static int luaB_tostring (lua_State *L) {
  moonL_checkany(L, 1);
  moonL_tolstring(L, 1, NULL);
  return 1;
}


static const luaL_Reg base_funcs[] = {
  {"assert", moonB_assert},
  {"collectgarbage", moonB_collectgarbage},
  {"dofile", moonB_dofile},
  {"error", moonB_error},
  {"getmetatable", moonB_getmetatable},
  {"ipairs", moonB_ipairs},
  {"loadfile", moonB_loadfile},
  {"load", moonB_load},
  {"next", moonB_next},
  {"pairs", moonB_pairs},
  {"pcall", moonB_pcall},
  {"print", moonB_print},
  {"warn", moonB_warn},
  {"rawequal", moonB_rawequal},
  {"rawlen", moonB_rawlen},
  {"rawget", moonB_rawget},
  {"rawset", moonB_rawset},
  {"select", moonB_select},
  {"setmetatable", moonB_setmetatable},
  {"tonumber", moonB_tonumber},
  {"tostring", luaB_tostring},
  {"type", moonB_type},
  {"xpcall", moonB_xpcall},
  /* placeholders */
  {LUA_GNAME, NULL},
  {"_VERSION", NULL},
  {NULL, NULL}
};


LUAMOD_API int moonopen_base (lua_State *L) {
  /* open lib into global table */
  lua_pushglobaltable(L);
  moonL_setfuncs(L, base_funcs, 0);
  /* set global _G */
  moon_pushvalue(L, -1);
  moon_setfield(L, -2, LUA_GNAME);
  /* set global _VERSION */
  lua_pushliteral(L, LUA_VERSION);
  moon_setfield(L, -2, "_VERSION");
  return 1;
}

