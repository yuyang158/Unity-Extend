/*
** $Id: lcode.h $
** Code generator for Lua
** See Copyright Notice in lua.h
*/

#ifndef lcode_h
#define lcode_h

#include "llex.h"
#include "lobject.h"
#include "lopcodes.h"
#include "lparser.h"


/*
** Marks the end of a patch list. It is an invalid value both as an absolute
** address, and as a list link (would link an element to itself).
*/
#define NO_JUMP (-1)


/*
** grep "ORDER OPR" if you change these enums  (ORDER OP)
*/
typedef enum BinOpr {
  /* arithmetic operators */
  OPR_ADD, OPR_SUB, OPR_MUL, OPR_MOD, OPR_POW,
  OPR_DIV, OPR_IDIV,
  /* bitwise operators */
  OPR_BAND, OPR_BOR, OPR_BXOR,
  OPR_SHL, OPR_SHR,
  /* string operator */
  OPR_CONCAT,
  /* comparison operators */
  OPR_EQ, OPR_LT, OPR_LE,
  OPR_NE, OPR_GT, OPR_GE,
  /* logical operators */
  OPR_AND, OPR_OR,
  OPR_NOBINOPR
} BinOpr;


/* true if operation is foldable (that is, it is arithmetic or bitwise) */
#define foldbinop(op)	((op) <= OPR_SHR)


#define luaK_codeABC(fs,o,a,b,c)	moonK_codeABCk(fs,o,a,b,c,0)


typedef enum UnOpr { OPR_MINUS, OPR_BNOT, OPR_NOT, OPR_LEN, OPR_NOUNOPR } UnOpr;


/* get (pointer to) instruction of given 'expdesc' */
#define getinstruction(fs,e)	((fs)->f->code[(e)->u.info])


#define luaK_setmultret(fs,e)	moonK_setreturns(fs, e, LUA_MULTRET)

#define luaK_jumpto(fs,t)	moonK_patchlist(fs, moonK_jump(fs), t)

LUAI_FUNC int moonK_code (FuncState *fs, Instruction i);
LUAI_FUNC int moonK_codeABx (FuncState *fs, OpCode o, int A, unsigned int Bx);
LUAI_FUNC int moonK_codeAsBx (FuncState *fs, OpCode o, int A, int Bx);
LUAI_FUNC int moonK_codeABCk (FuncState *fs, OpCode o, int A,
                                            int B, int C, int k);
LUAI_FUNC int moonK_isKint (expdesc *e);
LUAI_FUNC int moonK_exp2const (FuncState *fs, const expdesc *e, TValue *v);
LUAI_FUNC void moonK_fixline (FuncState *fs, int line);
LUAI_FUNC void moonK_nil (FuncState *fs, int from, int n);
LUAI_FUNC void moonK_reserveregs (FuncState *fs, int n);
LUAI_FUNC void moonK_checkstack (FuncState *fs, int n);
LUAI_FUNC void moonK_int (FuncState *fs, int reg, lua_Integer n);
LUAI_FUNC void moonK_dischargevars (FuncState *fs, expdesc *e);
LUAI_FUNC int moonK_exp2anyreg (FuncState *fs, expdesc *e);
LUAI_FUNC void moonK_exp2anyregup (FuncState *fs, expdesc *e);
LUAI_FUNC void moonK_exp2nextreg (FuncState *fs, expdesc *e);
LUAI_FUNC void moonK_exp2val (FuncState *fs, expdesc *e);
LUAI_FUNC int moonK_exp2RK (FuncState *fs, expdesc *e);
LUAI_FUNC void moonK_self (FuncState *fs, expdesc *e, expdesc *key);
LUAI_FUNC void moonK_indexed (FuncState *fs, expdesc *t, expdesc *k);
LUAI_FUNC void moonK_goiftrue (FuncState *fs, expdesc *e);
LUAI_FUNC void moonK_goiffalse (FuncState *fs, expdesc *e);
LUAI_FUNC void moonK_storevar (FuncState *fs, expdesc *var, expdesc *e);
LUAI_FUNC void moonK_setreturns (FuncState *fs, expdesc *e, int nresults);
LUAI_FUNC void moonK_setoneret (FuncState *fs, expdesc *e);
LUAI_FUNC int moonK_jump (FuncState *fs);
LUAI_FUNC void moonK_ret (FuncState *fs, int first, int nret);
LUAI_FUNC void moonK_patchlist (FuncState *fs, int list, int target);
LUAI_FUNC void moonK_patchtohere (FuncState *fs, int list);
LUAI_FUNC void moonK_concat (FuncState *fs, int *l1, int l2);
LUAI_FUNC int moonK_getlabel (FuncState *fs);
LUAI_FUNC void moonK_prefix (FuncState *fs, UnOpr op, expdesc *v, int line);
LUAI_FUNC void moonK_infix (FuncState *fs, BinOpr op, expdesc *v);
LUAI_FUNC void moonK_posfix (FuncState *fs, BinOpr op, expdesc *v1,
                            expdesc *v2, int line);
LUAI_FUNC void moonK_settablesize (FuncState *fs, int pc,
                                  int ra, int asize, int hsize);
LUAI_FUNC void moonK_setlist (FuncState *fs, int base, int nelems, int tostore);
LUAI_FUNC void moonK_finish (FuncState *fs);
LUAI_FUNC l_noret moonK_semerror (LexState *ls, const char *msg);


#endif
