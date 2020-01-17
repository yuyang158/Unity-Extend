using System;
using System.IO;
using Extend.AssetService;
using Extend.Common;
using UnityEngine;
using XLua;

namespace Extend {
    public class LuaVM : IService, IServiceUpdate {
        private LuaMemoryLeakChecker.Data leakData;
        private LuaFunction OnDestroy;
        public LuaEnv Default { get; private set; }

        public object[] LoadFileAtPath(string luaFileName ) {
            var ret = Default.DoString( $"return require '{luaFileName}'" );
            return ret;
        }

        public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.LUA_SERVICE;
        public void Initialize() {
            Default = new LuaEnv();
            Default.AddLoader( ( ref string filename ) => {
                filename = filename.Replace('.', '/');
                var service = CSharpServiceManager.Get<AssetService.AssetService>(CSharpServiceManager.ServiceType.ASSET_SERVICE);
                var assetRef = service.Load( $"Lua/{filename}", typeof(TextAsset) );
                if( assetRef == null || assetRef.AssetStatus != AssetRefObject.AssetStatus.DONE )
                    return null;
                filename += ".lua";
                return assetRef.GetTextAsset().bytes;
            } );

            LoadFileAtPath( "class" );
            OnDestroy = LoadFileAtPath( "PreRequest" )[0] as LuaFunction;
            leakData = Default.StartMemoryLeakCheck();
        }

        public void Destroy() {
            OnDestroy.Call();
            ReportLeak();
        }

        public void Update() {
            Default.Tick();
        }

        private void ReportLeak() {
            using( var writer = new StreamWriter(Application.dataPath + "/lua_memory_report.txt") ) {
                writer.Write(Default.MemoryLeakReport(leakData, 2));
            }
        }
    }
}