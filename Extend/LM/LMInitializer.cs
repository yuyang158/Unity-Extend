using System.Collections.Generic;
using UnityEngine;

namespace XLua.Extend.LM {
    public class LMInitializer : MonoBehaviour {
        private void Start() {
            var usedDocRoots = new Dictionary<string, LuaTable>();
            var bindings = GetComponentsInChildren<LuaMVVMBinding>();
            foreach( var binding in bindings ) {
                var splitPath = binding.path.Split( '.' );
                var rootName = splitPath[0];

                if( !usedDocRoots.TryGetValue( rootName, out var doc ) ) {
                    doc = LuaMVVM.Instance.GetDocRoot( rootName );
                    usedDocRoots.Add( rootName, doc );
                }

                var node = doc;
                for( var i = 1; i < splitPath.Length; i++ ) {
                    var path = splitPath[i];
                    if( i != splitPath.Length - 1 ) {
                        node = node.GetInPath<LuaTable>( path );
                    }
                    else {
                        var obj = node.GetInPath<object>( path );
                        binding.Change( obj );
                    }
                }
            }
        }
    }
}