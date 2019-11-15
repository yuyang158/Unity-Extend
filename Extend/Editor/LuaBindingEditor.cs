using System;
using UnityEditor;
using System.Reflection;
using UnityEditorInternal;
using UnityEngine;

namespace Extend {
    [CustomEditor( typeof( LuaBinding ) )]
    public class LuaBindingEditor : UnityEditor.Editor {
        private ReorderableList bindingList;
        private void OnEnable() {
            var bindingProp = serializedObject.FindProperty( "BindingContainer" );
            bindingList = new ReorderableList( serializedObject, bindingProp );
            bindingList.drawHeaderCallback += rect => {
                EditorGUI.LabelField( rect, "Component Binding" );
            };

            var luaBinding = target as LuaBinding;
            bindingList.onAddCallback += _ => {
                luaBinding.BindingContainer.Add( new LuaBinding.LuaUnityBinding() );
                serializedObject.Update();
            };
            
            var bindingType = typeof(LuaBinding.LuaUnityBinding);
            bindingList.drawElementCallback += (rect, index, active, focused) => {
                rect.height = EditorGUIUtility.singleLineHeight;
                var elementProp = bindingProp.GetArrayElementAtIndex( index );
                var fields = bindingType.GetFields( BindingFlags.Public | BindingFlags.Instance );
                foreach( var field in fields ) {
                    var prop = elementProp.FindPropertyRelative( field.Name );
                    EditorGUI.PropertyField( rect, prop );
                    rect.y += EditorGUIUtility.singleLineHeight;
                }

                var binding = luaBinding.BindingContainer[index];
                if( binding.UnityObject ) {
                    Component[] components;
                    int selectedIndex;
                    if( binding.UnityObject is GameObject gameObject ) {
                        components = gameObject.GetComponents<Component>();
                        selectedIndex = components.Length;
                    }
                    else {
                        var component = ( binding.UnityObject as Component );
                        components = component.GetComponents<Component>();
                        selectedIndex = Array.IndexOf( components, component );
                    }
                    var componentNames = new string[components.Length + 1];
                    for( var i = 0; i < components.Length; i++ ) {
                        componentNames[i] = components[i].GetType().Name;
                    }

                    componentNames[componentNames.Length - 1] = "GameObject";
                    
                    var selectIndex = EditorGUI.Popup( rect, "可选组件", selectedIndex, componentNames );
                    if( selectIndex < components.Length ) {
                        binding.UnityObject = components[selectIndex];
                    }
                    else {
                        binding.UnityObject = components[0].gameObject;
                    }
                }
                
                serializedObject.ApplyModifiedProperties();
            };

            bindingList.elementHeight = EditorGUIUtility.singleLineHeight * 3;
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            bindingList.DoLayoutList();
        }
    }
}
