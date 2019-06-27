using UnityEngine;
using UnityEditor;
using System;

namespace XLua.Extend {
    [CustomEditor( typeof( LuaBinding ) )]
    public class LuaBindingEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if( GUILayout.Button( "Reload Lua File" ) ) {
                var target = serializedObject.targetObject as LuaBinding;
                var rets = LuaVM.Default.LoadTmpFileAtPath( target.luaFileName );
                var bindingConfig = rets[1] as LuaTable;
                var old = target.bindingContainer;
                target.bindingContainer = new LuaBinding.StringGOSerializableDictionary();

                bindingConfig.ForEach( ( string name, LuaTable typ ) => {
                    if( old.TryGetValue( name, out LuaBinding.LuaUnityBinding binding ) &&
                        !( binding.type == null && binding.go == null ) ) {
                        target.bindingContainer.Add( name, binding );
                    }
                    else {
                        target.bindingContainer.Add( name, new LuaBinding.LuaUnityBinding() {
                            type = typ.GetInPath<Type>( "UnderlyingSystemType" )
                        } );
                    }
                } );
                EditorUtility.SetDirty( target );
            }
        }
    }
}
