using SerializableCollections;
using System;
using UnityEngine;

namespace XLua.Extend {
    public class LuaBinding : MonoBehaviour {
        public string luaFileName;

        [Serializable]
        public class LuaUnityBinding {
            [SerializeField]
            public Type type;
            [SerializeField]
            public MonoBehaviour go;
        }

        [Serializable]
        public class StringGOSerializableDictionary : SerializableDictionary<string, LuaUnityBinding> {
        }

        public StringGOSerializableDictionary bindingContainer;

        protected LuaTable bindInstance;
        protected virtual void Awake() {
            var ret = LuaVM.Default.LoadFileAtPath( luaFileName );
            var classTable = ret[0] as LuaTable;
            var constructor = classTable.Get<LuaFunction>( "new" );
            bindInstance = constructor.Call( gameObject )[0] as LuaTable;

            if( bindingContainer != null ) {
                foreach( var item in bindingContainer ) {
                    if( item.Value.go )
                        bindInstance.SetInPath( item.Key, item.Value.go );
                }
            }

            var awakeFunc = bindInstance.Get<LuaFunction>( "awake" );
            awakeFunc.Call( bindInstance );
        }
    }
}


