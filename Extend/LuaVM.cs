using UnityEngine;

namespace XLua.Extend {
    public static class LuaVM {
        static LuaVM() {
            Default = new LuaEnv();
            Default.customLoaders.Add( ( ref string filename ) => {
                var asset = Resources.Load<TextAsset>( "Lua/" + filename + ".lua" );
                return asset.bytes;
            } );

            Default.LoadFileAtPath( "class" );
        }

        public static LuaEnv Default { get; private set; }

        public static object[] LoadFileAtPath( this LuaEnv env, string luaFileName ) {
            var rets = env.DoString( string.Format( "return require '{0}'", luaFileName ) );
            return rets;
        }

#if UNITY_EDITOR
        public static object[] LoadTmpFileAtPath( this LuaEnv env, string luaFileName ) {
            var asset = Resources.Load<TextAsset>( "Lua/" + luaFileName + ".lua" );
            var rets = env.DoString( asset.bytes );
            return rets;
        }
#endif
    }
}