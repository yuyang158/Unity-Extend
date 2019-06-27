using System.Collections;
using UnityEditor;
using UnityEngine;

namespace SerializableCollections {
    public abstract class SerializableDictionaryPropertyDrawer : PropertyDrawer {
        protected bool foldout = true;

        public override float GetPropertyHeight( SerializedProperty property, GUIContent label ) {
            var height = base.GetPropertyHeight( property, label );

            var target = property.serializedObject.targetObject;
            if( !( fieldInfo.GetValue( target ) is IDictionary dictionary ) ) return height;

            return ( foldout )
                ? ( dictionary.Count + 1 ) * EditorGUIUtility.singleLineHeight
                : EditorGUIUtility.singleLineHeight;
        }
    }

    [CustomPropertyDrawer( typeof( XLua.Extend.LuaBinding.StringGOSerializableDictionary ) )]
    public class ExtendedSerializableDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer {
        public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {
            position.height = EditorGUIUtility.singleLineHeight;
            var target = property.serializedObject.targetObject;
            if( !( fieldInfo.GetValue( target ) is IDictionary dictionary ) ) return;

            foldout = EditorGUI.Foldout( position, foldout, label, true );
            EditorGUI.LabelField( position, label, new GUIContent() { text = "Count:" + dictionary.Count } );
            if( foldout ) {
                foreach( DictionaryEntry item in dictionary ) {
                    position = new Rect( position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, position.height );
                    var name = item.Key as string;
                    var binding = item.Value as XLua.Extend.LuaBinding.LuaUnityBinding;
                    var go = EditorGUI.ObjectField( position, new GUIContent( name ), binding.go, binding.type, true ) as MonoBehaviour;
                    if( binding.go != go ) {
                        binding.go = go;
                        EditorUtility.SetDirty( target );
                    }
                }
            }
        }
    }
}