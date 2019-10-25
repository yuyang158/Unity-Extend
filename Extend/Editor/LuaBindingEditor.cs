using UnityEngine;
using UnityEditor;
using System;

namespace XLua.Extend {
    [CustomEditor( typeof( LuaBinding ) )]
    public class LuaBindingEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if( GUILayout.Button( "Reload Lua File" ) ) {
                var targetObject = serializedObject.targetObject as LuaBinding;
                var retValues = LuaVM.Default.LoadTmpFileAtPath( targetObject.luaFileName );
                if( retValues.Length < 1 ) {
                    return;
                }

                var klass = retValues[0] as LuaTable;
                var bindingConfig = klass.Get<LuaTable>( "binding" );
                var old = targetObject.bindingContainer;
                targetObject.bindingContainer = new LuaBinding.StringGOSerializableDictionary();

                bindingConfig.ForEach( ( string varName, LuaTable typ ) => {
                    if( old.TryGetValue( varName, out var binding ) &&
                        !( binding.type == null && binding.go == null ) ) {
                        targetObject.bindingContainer.Add( varName, binding );
                    }
                    else {
                        targetObject.bindingContainer.Add( varName, new LuaBinding.LuaUnityBinding() {
                            type = typ.GetInPath<Type>( "UnderlyingSystemType" )
                        } );
                    }
                } );
                EditorUtility.SetDirty( targetObject );
            }
        }
    }

    [CustomEditor( typeof( LuaBinding_Update ) )]
    public class LuaBinding_UpdateEditor : LuaBindingEditor {

    }
}
