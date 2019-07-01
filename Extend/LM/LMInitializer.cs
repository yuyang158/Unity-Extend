using System.Collections.Generic;
using UnityEngine;

namespace XLua.Extend.LM {
    public class LMInitializer : MonoBehaviour {
        void Start() {
            Dictionary<string, LuaTable> usedDocRoots = new Dictionary<string, LuaTable>();
            var bindings = GetComponentsInChildren<LuaMVVMBinding>();
            foreach( var binding in bindings ) {
                var pathes = binding.path.Split( '.' );
                var rootName = pathes[0];

                if( !usedDocRoots.TryGetValue( rootName, out LuaTable doc ) ) {
                    doc = LuaMVVM.Instance.GetDocRoot( rootName );
                    usedDocRoots.Add( rootName, doc );
                }

                var node = doc;
                for( int i = 1; i < pathes.Length; i++ ) {
                    var path = pathes[i];
                    if( i != pathes.Length - 1 ) {
                        node = node.GetInPath<LuaTable>( path );
                    }
                    else {
                        object obj = node.GetInPath<object>( path );
                        binding.Change( obj );
                    }
                }
            }
        }
    }
}