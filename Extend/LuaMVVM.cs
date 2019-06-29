using System.Collections.Generic;
using UnityEngine;

namespace XLua.Extend {
    [CSharpCallLua]
    public class LuaMVVM : MonoBehaviour {
        public class RandomList<T> : List<T> {
            public bool SwapRemove( T item ) {
                var index = IndexOf( item );
                if( index < 0 ) {
                    return false;
                }

                if( Count == 1 ) {
                    Clear();
                }
                else {
                    var lastIndex = Count - 1;
                    if( index != lastIndex ) {
                        var last = this[Count - 1];
                        this[index] = last;
                    }

                    RemoveAt( lastIndex );
                }
                return true;
            }
        }

        public class MVVMBindingList : RandomList<LuaMVVMBinding> { }

        static LuaMVVM() {
            var go = new GameObject {
                name = "mvvm"
            };
            Instance = go.AddComponent<LuaMVVM>();
            DontDestroyOnLoad( go );
        }

        public static LuaMVVM Instance {
            get;
            private set;
        }

        private Dictionary<string, MVVMBindingList> bindings = new Dictionary<string, MVVMBindingList>();
        private delegate Dictionary<string, Dictionary<string, object>> FetchChangeMethod();
        private FetchChangeMethod fetchMethod;

        void Awake() {
            var rets = LuaVM.Default.LoadFileAtPath( "mvvm" );
            var module = rets[0] as LuaTable;
            fetchMethod = module.GetInPath<FetchChangeMethod>( "fetch_all" );
        }

        public void RegisterBinding( LuaMVVMBinding binding ) {
            if( !bindings.TryGetValue( binding.path, out MVVMBindingList list ) ) {
                list = new MVVMBindingList();
                bindings.Add( binding.path, list );
            }
            list.Add( binding );
        }

        public void UnreigsterBinding( LuaMVVMBinding binding ) {
            if( bindings.TryGetValue( binding.path, out MVVMBindingList list ) ) {
                list.SwapRemove( binding );
            }
        }

        void LateUpdate() {
            var changes = fetchMethod();
            foreach( var item in changes ) {
                var name = item.Key;
                foreach( var dirtyValue in item.Value ) {
                    if( bindings.TryGetValue( dirtyValue.Key, out MVVMBindingList list ) ) {
                        foreach( var binding in list ) {
                            binding.Change( dirtyValue.Value );
                        }
                    }
                }
            }
        }
    }
}
