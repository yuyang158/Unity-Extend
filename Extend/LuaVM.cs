using Extend.AssetService;
using Extend.Common;
using UnityEngine;
using XLua;

namespace Extend {
    public static class LuaVM {
        static LuaVM() {
            Default = new LuaEnv();
            Default.AddLoader( ( ref string filename ) => {
                var service = CSharpServiceManager.Get<AssetService.AssetService>(CSharpServiceManager.ServiceType.ASSET_SERVICE);
                var assetRef = service.Load( $"Lua/{filename}", typeof(TextAsset) );
                if( assetRef == null || assetRef.AssetStatus != AssetRefObject.AssetStatus.DONE )
                    return null;
                filename += ".lua";
                return assetRef.GetTextAsset().bytes;
            } );

            Default.LoadFileAtPath( "class" );
            Default.LoadFileAtPath( "PreRequest" );
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