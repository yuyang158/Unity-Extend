using System;
using UnityEngine;

namespace XLua.Extend {
    public static class LuaVM {
        static LuaVM() {
            Default = new LuaEnv();
            Default.AddLoader( ( ref string filename ) => {
                var asset = Resources.Load<TextAsset>( $"Lua/{filename}" );
                filename += ".lua";
                return asset.bytes;
            } );

            Default.LoadFileAtPath( "class" );
            Default.LoadFileAtPath( "PreRequest" );

            LuaDLL.Lua.lua_atpanic( Default.L, ptr => {
                Debug.LogError( "PANIC" );
                return 0;
            } );
        }

        public static LuaEnv Default { get; private set; }

        public static object[] LoadFileAtPath( this LuaEnv env, string luaFileName ) {
            var ret = env.DoString( $"return require '{luaFileName}'" );
            return ret;
        }

#if UNITY_EDITOR
        public static object[] LoadTmpFileAtPath( this LuaEnv env, string luaFileName ) {
            var asset = Resources.Load<TextAsset>( $"Lua/{luaFileName}" );
            var ret = env.DoString( asset.bytes );
            return ret;
        }
#endif
    }
}