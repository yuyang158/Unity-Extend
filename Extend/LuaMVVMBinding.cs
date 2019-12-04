using System;
using Extend.Common;
using UnityEngine;

namespace Extend {
    public abstract class LuaMVVMBinding : MonoBehaviour {
        public string path;

        private void Awake() {
            
        }

        protected virtual void OnEnable() {
            var luaMvvm = CSharpServiceManager.Get<LuaMVVM>( CSharpServiceManager.ServiceType.MVVM_SERVICE );
            luaMvvm.RegisterBinding( this );
        }

        protected virtual void OnDisable() {
            var luaMvvm = CSharpServiceManager.Get<LuaMVVM>( CSharpServiceManager.ServiceType.MVVM_SERVICE );
            luaMvvm.UnregisterBinding( this );
        }

        public abstract void Change( object value );
    }

    public abstract class LMBooleanBinding : LuaMVVMBinding {
        public sealed override void Change( object value ) {
            var b = (bool)value;
            ChangeBoolean( b );
        }

        protected abstract void ChangeBoolean( bool v );
    }

    public abstract class LMFloatBinding : LuaMVVMBinding {
        public override void Change( object value ) {
            var f = (float)value;
            ChangeFloat( f );
        }

        protected abstract void ChangeFloat( float v );
    }

    public abstract class LMIntegerBinding : LuaMVVMBinding {
        public override void Change( object value ) {
            long l = (long)value;
            ChangeInteger( l );
        }

        protected abstract void ChangeInteger( long v );
    }
}