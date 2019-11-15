using UnityEngine;

namespace Extend.LM {
    public class LMReflectionBinding : LuaMVVMBinding {
        public Component component;
        public string fieldName;
        public string propertyName;

        public override void Change( object value ) {
            if( !string.IsNullOrEmpty( fieldName ) ) {
                var fieldInfo = component.GetType().GetField( fieldName );
                if( fieldInfo == null ) {
                    Debug.LogWarningFormat( "No field named : {0}", fieldName );
                    return;
                }

                fieldInfo.SetValue( component, value );
            }

            if( !string.IsNullOrEmpty( propertyName ) ) {
                var propertyInfo = component.GetType().GetProperty( propertyName );
                if( propertyInfo == null ) {
                    Debug.LogWarningFormat( "No property named : {0}", fieldName );
                    return;
                }

                propertyInfo.SetValue( component, value );
            }
        }

        private void Start() {
            Debug.Assert( component );
        }
    }
}